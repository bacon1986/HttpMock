﻿using System;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
    [TestFixture]
    public class RequestHandlerTests
    {
        [Test]
        public void Applies_One_Constraint()
        {
            var doesNotContainBlahConstraint = new Func<string, bool>(uri => uri.Contains("blah") == false);

            var h = new RequestHandler("", null);

            h.WithUrlConstraint(doesNotContainBlahConstraint);

            var uriWithBlah = "http://www.blah.com";

            Assert.That(h.CanVerifyConstraintsFor(uriWithBlah, ""), Is.EqualTo(false));
        }

        [Test]
        public void Applies_multiple_constraints()
        {
            var doesNotContainBlahConstraint = new Func<string, bool>(uri => uri.Contains("blah") == false);
            var doesNotContainHibriConstraint = new Func<string, bool>(uri => uri.Contains("hibri") == false);
            var doesNotContainGoncaloConstraint = new Func<string, bool>(uri => uri.Contains("goncalo") == false);

            var h = new RequestHandler("", null);

            h.WithUrlConstraint(doesNotContainBlahConstraint);
            h.WithUrlConstraint(doesNotContainHibriConstraint);
            h.WithUrlConstraint(doesNotContainGoncaloConstraint);

            var uriWithBlah = "http://www.xyz.com/moomins/goncalo";

            Assert.That(h.CanVerifyConstraintsFor(uriWithBlah, ""), Is.EqualTo(false));
        }
    }
}