using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
    public class StubConstraintsTests
    {
        private IHttpServer _httpMockRepository;
        private WebClient _wc;
        private IHttpServer _stubHttp;
        private string _hostUrl;

        [SetUp]
        public void SetUp()
        {
            _hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
            _httpMockRepository = HttpMockRepository.At(_hostUrl);
            _wc = new WebClient();
            _stubHttp = _httpMockRepository.WithNewContext();
        }

        [Test]
        public void Constraints_can_be_applied_to_urls()
        {
            _stubHttp
                .Stub(x => x.Post("/firsttest"))
                .WithUrlConstraint(url => url.Contains("/blah/blah") == false)
                .Return("<Xml>ShouldntBeReturned</Xml>")
                .OK();

            try
            {
                _wc.UploadString(string.Format("{0}/firsttest/blah/blah", _hostUrl), "x");

                Assert.Fail("Should have 404d");
            }
            catch (WebException ex)
            {
                Assert.That(((HttpWebResponse)ex.Response).StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            }
        }

        [Test]
        public void Constraints_can_be_applied_to_bodies()
        {
            var expectedResult = "<result>OK</result>";
            _stubHttp
                .Stub(x => x.Post("/firsttest"))
                .WithBodyConstraint(body => body.Contains("<search>bar</search>"))
                .Return(expectedResult)
                .OK();

            try
            {
                _wc.UploadString(string.Format("{0}/firsttest", _hostUrl), "<search>foo</search>");

                Assert.Fail("Should have 404d");
            }
            catch (WebException ex)
            {
                if (ex.Response == null) throw;
                Assert.That(((HttpWebResponse)ex.Response).StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            }

            var actualResult = _wc.UploadString(string.Format("{0}/firsttest", _hostUrl), "<search>bar</search>");

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}