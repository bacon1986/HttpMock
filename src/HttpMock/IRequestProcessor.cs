using Kayak.Http;
using System.Collections.Generic;

namespace HttpMock
{
	public interface IRequestProcessor : IHttpRequestDelegate
	{
		IRequestVerify FindHandler(string method, string path);
		void Add(RequestHandler requestHandler);
		void ClearHandlers();
		string WhatDoIHave();
        IEnumerable<RequestHandler> GetAllRequests();

    }
}