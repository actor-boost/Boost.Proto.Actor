using System.Diagnostics;
using AutoFixture.Xunit2;
using FluentAssertions;
using LanguageExt;
using Moq;
using Proto;
using Xunit;
using static LanguageExt.Prelude;


namespace Boost.Proto.Actor.Opentelemetry.Tests
{
    public class OpenTelemetryActorContextDecoratorSpec
    {
        [Theory, AutoData]
        public void ResponseEitherCheck(Mock<IContext> context)
        {
            Either<string, Unit> sut = unit;

            var ret = sut switch
            {
                IEither m => m.MatchUntyped(x => x.GetType().Name,
                                            x => x.GetType().Name),
            };

            ret.Should().Be("Unit");

        }
    }
}
