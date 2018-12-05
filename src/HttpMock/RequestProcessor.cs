using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
    public class RequestProcessor : IRequestProcessor
    {
        private static readonly ILog _log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IRequestHandlerList _handlers;
        private readonly RequestMatcher _requestMatcher;
        private static int missingHandlerCount = 0;

        public static int MissingHandlerCount { get => missingHandlerCount; } 

        public RequestProcessor(IMatchingRule matchingRule, IRequestHandlerList requestHandlers)
        {
            _handlers = requestHandlers;
            _requestMatcher = new RequestMatcher(matchingRule);
        }

        public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response)
        {
            _log.DebugFormat("Start Processing request for : {0}:{1}", request.Method, request.Uri);
            if (GetHandlerCount() < 1)
            {
                ReturnHttpMockNotFound(response);
                return;
            }

            if (request.HasBody())
            {
                body.Connect(new BufferedConsumer(
                    bufferedBody =>
                    {
                        _log.DebugFormat("Body: {0}", bufferedBody);
                        Continue(request, bufferedBody, response);
                    },
                    error =>
                    {
                        _log.DebugFormat("Error while reading body {0}", error.Message);
                        Continue(request, "", response);
                    }
                ));
            }
            else
            {
                Continue(request, "", response);
            }
        }

        private void Continue(HttpRequestHead request, string body, IHttpResponseDelegate response)
        {
            var handler = _requestMatcher.Match(request, body, _handlers);

            if (handler == null)
            {
                _log.DebugFormat("No Handlers matched");
                ReturnHttpMockNotFound(response);
                return;
            }
            HandleRequest(request, body, response, handler);
        }

        private static async void HandleRequest(HttpRequestHead request, string body, IHttpResponseDelegate response, IRequestHandler handler)
        {
            _log.DebugFormat("Matched a handler {0}:{1} {2}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));

            if (handler.ResponseDelay > TimeSpan.Zero)
            {
                await Task.Delay(handler.ResponseDelay);
            }
            IDataProducer dataProducer = GetDataProducer(request, handler);

            response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
            handler.RecordRequest(request, body);

            _log.DebugFormat("End Processing request for : {0}:{1}", request.Method, request.Uri);
        }

        private static IDataProducer GetDataProducer(HttpRequestHead request, IRequestHandler handler)
        {
            return request.Method != "HEAD" ? handler.ResponseBuilder.BuildBody(request.Headers) : null;
        }

        private int GetHandlerCount()
        {
            return _handlers.Count();
        }

        public IRequestVerify FindHandler(string method, string path)
        {
            return (IRequestVerify)_handlers.Where(x => x.Path == path && x.Method == method).FirstOrDefault();
        }

        private static string DumpQueryParams(IDictionary<string, string> queryParams)
        {
            var sb = new StringBuilder();
            foreach (var param in queryParams)
            {
                sb.AppendFormat("{0}={1}&", param.Key, param.Value);
            }
            return sb.ToString();
        }

        private static void ReturnHttpMockNotFound(IHttpResponseDelegate response)
        {
            missingHandlerCount++;

            var dictionary = new Dictionary<string, string>
            {
                {HttpHeaderNames.ContentLength, "0"},
                {"X-HttpMockError", "No handler found to handle request"}
            };

            var notFoundResponse = new HttpResponseHead
            { Status = string.Format("{0} {1}", 404, "NotFound"), Headers = dictionary };
            response.OnResponse(notFoundResponse, null);
        }

        public void ClearHandlers()
        {
            _handlers = new RequestHandlerList();
            missingHandlerCount = 0;
        }

        public void Add(RequestHandler requestHandler)
        {
            _handlers.Add(requestHandler);
        }

        public string WhatDoIHave()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Handlers:");
            foreach (RequestHandler handler in _handlers)
            {
                stringBuilder.Append(handler.ToString());
            }
            return stringBuilder.ToString();
        }

        public IEnumerable<RequestHandler> GetAllRequests()
        {
            foreach (RequestHandler handler in _handlers)
            {
                yield return handler;
            }
        }
    }
}