using System;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		IRequestStub Stub(Func<RequestHandlerFactory, IRequestStub> func);
		IHttpServer WithNewContext();
		void Start();
		string WhatDoIHave();
        int HandlerMissCount { get; }
        bool IsAvailable();
		IRequestProcessor GetRequestProcessor();
        string Uri { get; }
	}
}