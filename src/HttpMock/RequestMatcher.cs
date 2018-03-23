using System.Collections.Generic;
using System.Linq;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
    public interface IRequestMatcher
    {
        IRequestHandler Match(HttpRequestHead request, string body, IEnumerable<IRequestHandler> requestHandlerList);
    }

    public class RequestMatcher : IRequestMatcher
    {
        private readonly IMatchingRule _matchingRule;

        public RequestMatcher(IMatchingRule matchingRule)
        {
            _matchingRule = matchingRule;
        }

        public IRequestHandler Match(HttpRequestHead request, string body, IEnumerable<IRequestHandler> requestHandlerList)
        {
            var matches = requestHandlerList
                .Where(handler => _matchingRule.IsEndpointMatch(handler, request))
                .Where(handler => handler.CanVerifyConstraintsFor(request.Uri, body));

            return matches.FirstOrDefault();
        }
    }
}