using System;
using System.Threading.Tasks;
using Common.Extensions;
using FluentAssertions;
using LanguageExt;
using Moq;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Frontend.Unit.Tests.Extensions
{
    public class FunctionalExtensionTests
    {
        public interface ILog
        {
            void Log(string message);
        }

        public class CanBeDisposed : IDisposable
        {
            private readonly ILog _logger;

            public CanBeDisposed(ILog logger)
            {
                _logger = logger;
            }

            public static bool GetTrue() => true;

            public static Task<bool> GetTrueAsync() => Task.FromResult(true);

            public static Task SetTrueAsync(ref bool value)
            {
                Task.Delay(1000);
                value = true;
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                _logger.Log("dispose");
            }
        }

        public class CanBeDisposedAlt : IDisposable
        {
            private readonly ILog _logger;

            public CanBeDisposedAlt(ILog logger)
            {
                _logger = logger;
            }

            public static string GetTrue() => CanBeDisposed.GetTrue().ToString();

            public static Task<string> GetTrueAsync() => Task.FromResult(CanBeDisposed.GetTrue().ToString());

            public static Task SetTrueAsync(ref string value)
            {
                Task.Delay(1000);
                value = CanBeDisposed.GetTrue().ToString();
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                _logger.Log("dispose-alt");
            }
        }

        public struct ValueTypeStruct
        {
            public bool Success { get; set; }
        }

        [Test]
        public void Using_WithActionDisposable_ShouldCallWithObject()
        {
            var log = new Mock<ILog>();
            var called = false;

            var result = FunctionalExtension.Using(new CanBeDisposed(log.Object),
                               cbd => { called = CanBeDisposed.GetTrue(); });

            result.Should().Be(LanguageExt.Unit.Default);
            called.Should().BeTrue();
            log.Verify(m => m.Log("dispose"), Times.Once);
        }

        [Test]
        public void UsingAsync_WithActionDisposable_ShouldCallWithObject()
        {
            var log = new Mock<ILog>();
            var called = false;

            var result = FunctionalExtension.UsingAsync(new CanBeDisposed(log.Object),
                                    cbd => CanBeDisposed.SetTrueAsync(ref called)).Result;

            result.Should().Be(LanguageExt.Unit.Default);
            called.Should().BeTrue();
            log.Verify(m => m.Log("dispose"), Times.Once);
        }

        [Test]
        public void Using_WithFunctionDisposable_ShouldCallWithObject()
        {
            var log = new Mock<ILog>();

            var result = FunctionalExtension.Using(new CanBeDisposed(log.Object),
                               cbd => CanBeDisposed.GetTrue());

            result.Should().BeTrue();
            log.Verify(m => m.Log("dispose"), Times.Once);
        }

        [Test]
        public void UsingAsync_WithFunctionDisposable_ShouldCallWithObject()
        {
            var log = new Mock<ILog>();

            var result = FunctionalExtension.UsingAsync(new CanBeDisposed(log.Object),
                                    cbd => CanBeDisposed.GetTrueAsync()).Result;

            result.Should().BeTrue();
            log.Verify(m => m.Log("dispose"), Times.Once);
        }

        [Test]
        public void Using_WithActionWithTwoDisposable_ShouldCallWithObjects()
        {
            var log = new Mock<ILog>();
            var called = false;
            var calledAlt = "False";

            var result = FunctionalExtension.Using(new CanBeDisposed(log.Object),
                               cbd => new CanBeDisposedAlt(log.Object),
                               (cbd, cbda) =>
                               {
                                   called = CanBeDisposed.GetTrue();
                                   calledAlt = CanBeDisposedAlt.GetTrue();
                               });

            result.Should().Be(LanguageExt.Unit.Default);
            called.Should().BeTrue();
            calledAlt.Should().Be("True");
            log.Verify(m => m.Log("dispose"), Times.Once);
            log.Verify(m => m.Log("dispose-alt"), Times.Once);
        }

        [Test]
        public void UsingAsync_WithActionWithTwoDisposable_ShouldCallWithObjects()
        {
            var log = new Mock<ILog>();
            var called = false;
            var calledAlt = "false";

            var result = FunctionalExtension.UsingAsync(new CanBeDisposed(log.Object),
                                    cbd => new CanBeDisposedAlt(log.Object),
                                    async (cbd, cbda) =>
                                    {
                                        await CanBeDisposed.SetTrueAsync(ref called);
                                        await CanBeDisposedAlt.SetTrueAsync(ref calledAlt);
                                    }).Result;

            result.Should().Be(LanguageExt.Unit.Default);
            called.Should().BeTrue();
            calledAlt.Should().Be("True");
            log.Verify(m => m.Log("dispose"), Times.Once);
            log.Verify(m => m.Log("dispose-alt"), Times.Once);
        }

        [Test]
        public void Using_WithFunctionWithTwoDisposable_ShouldCallWithObject()
        {
            var log = new Mock<ILog>();

            var (result1, result2) = FunctionalExtension.Using(new CanBeDisposed(log.Object),
                                           cbd => new CanBeDisposedAlt(log.Object),
                                           (cbd, cbda) =>
                                           (
                                               CanBeDisposed.GetTrue(),
                                               CanBeDisposedAlt.GetTrue()
                                           ));

            result1.Should().BeTrue();
            result2.Should().Be("True");
            log.Verify(m => m.Log("dispose"), Times.Once);
            log.Verify(m => m.Log("dispose-alt"), Times.Once);
        }

        [Test]
        public void UsingAsync_WithFunctionWithTwoDisposable_ShouldCallWithObject()
        {
            var log = new Mock<ILog>();

            var (result1, result2) = FunctionalExtension.UsingAsync(new CanBeDisposed(log.Object),
                                                cbd => new CanBeDisposedAlt(log.Object),
                                                async (cbd, cbda) =>
                                                (
                                                    await CanBeDisposed.GetTrueAsync(),
                                                    await CanBeDisposedAlt.GetTrueAsync()
                                                )).Result;

            result1.Should().BeTrue();
            result2.Should().Be("True");
            log.Verify(m => m.Log("dispose"), Times.Once);
            log.Verify(m => m.Log("dispose-alt"), Times.Once);
        }

        [Test]
        public void ToOption_WithNull_ShouldBeNone()
        {
            var option = ((CanBeDisposed)null).ToOption();

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEither_WithNull_ShouldBeLeft()
        {
            var either = ((CanBeDisposed)null).MakeEither(10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherLeftFunc_WithNull_ShouldBeLeft()
        {
            var either = ((CanBeDisposed)null).MakeEither(() => 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOption_WithValueAndNoneFunction_ShouldBeNone()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var option = canBeDisposed.ToOption(_ => true);

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEither_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(_ => true, 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherLeftFunc_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(_ => true, () => 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOption_WithValue_ShouldBeSome()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var option = canBeDisposed.ToOption();

            option.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEither_WithValue_ShouldBeRight()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(10);

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherLeftFunc_WithValue_ShouldBeRight()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(() => 10);

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void ToOptionMapping_WithNull_ShouldBeNone()
        {
            var option = ((CanBeDisposed)null).ToOption(_ => 1, _ => true);

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherMapping_WithNull_ShouldBeLeft()
        {
            var either = ((CanBeDisposed)null).MakeEither(_ => 1, _ => true, 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherLeftFuncMapping_WithNull_ShouldBeLeft()
        {
            var either = ((CanBeDisposed)null).MakeEither(_ => 1, _ => true, () => 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOptionMapping_WithBool_ShouldHandleProperly()
        {
            false.ToOption(_ => _).IsNone.Should().BeFalse();
            true.ToOption(_ => _).IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherMapping_WithBool_ShouldHandleProperly()
        {
            false.MakeEither(_ => _, 10).IsLeft.Should().BeFalse();
            true.MakeEither(_ => _, 10).IsLeft.Should().BeTrue();
        }

        [Test]
        public void MakeEitherLeftFuncMapping_WithBool_ShouldHandleProperly()
        {
            false.MakeEither(_ => _, () => 10).IsLeft.Should().BeFalse();
            true.MakeEither(_ => _, () => 10).IsLeft.Should().BeTrue();
        }

        [Test]
        public void ToOptionMapping_WithValueType_ShouldHandleProperly()
        {
            var value = new ValueTypeStruct { Success = true };

            var option = value.ToOption(_ => _.Success == false);

            option.IsNone.Should().BeFalse();
        }

        [Test]
        public void MakeEitherMapping_WithValueType_ShouldHandleProperly()
        {
            var value = new ValueTypeStruct { Success = true };

            var either = value.MakeEither(_ => _.Success == false, 10);

            either.IsLeft.Should().BeFalse();
        }

        [Test]
        public void MakeEitherLeftFuncMapping_WithValueType_ShouldHandleProperly()
        {
            var value = new ValueTypeStruct { Success = true };

            var either = value.MakeEither(_ => _.Success == false, () => 10);

            either.IsLeft.Should().BeFalse();
        }

        [Test]
        public void ToOptionMapping_WithDefaultValueType_ShouldHandleProperly()
        {
            var value = new ValueTypeStruct();

            var option = value.ToOption(_ => _.Success);

            option.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEitherMapping_WithDefaultValueType_ShouldHandleProperly()
        {
            var value = new ValueTypeStruct();

            var either = value.MakeEither(_ => _.Success, 10);

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherLeftFuncMapping_WithDefaultValueType_ShouldHandleProperly()
        {
            var value = new ValueTypeStruct();

            var either = value.MakeEither(_ => _.Success, () => 10);

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void ToOptionMapping_WithValueAndNoneFunction_ShouldBeNone()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var option = canBeDisposed.ToOption(_ => 1, _ => true);

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherMapping_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(_ => 1, _ => true, 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherLeftFuncMapping_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(_ => 1, _ => true, () => 10);

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOptionMapping_WithValue_ShouldBeSome()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var option = canBeDisposed.ToOption(_ => 1, _ => false);

            option.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEitherMapping_WithValue_ShouldBeRight()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(_ => 1, _ => false, 10);

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherLeftFuncMapping_WithValue_ShouldBeRight()
        {
            var canBeDisposed = new CanBeDisposed(new Mock<ILog>().Object);

            var either = canBeDisposed.MakeEither(_ => 1, _ => false, () => 10);

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void Tee_Should_Transform()
        {
            var result = "any".Tee(s => "teezed");

            result.Should().Be("teezed");
        }

        internal class TestClass
        {
            internal string Status;
        }

        [Test]
        public void Tee_WithAction_Should_ReturnSameObject()
        {
            var input = new TestClass
            {
                Status = "initial"
            };

            var result = input.Tee(i => i.Status = "final");

            result.Should().Be(input);
            result.Status.Should().Be("final");
        }

        [TestCase(true, "teezed")]
        [TestCase(false, "any")]
        public void TeeWhen_WithTrueOrFalseCondition_ShouldTransformOrNot(bool whenResult, string expected)
        {
            var result = "any".TeeWhen(s => "teezed", () => whenResult);

            result.Should().Be(expected);
        }

        [TestCase(true, "teezed")]
        [TestCase(false, "any")]
        public void TeeWhen_WithTrueOrFalseConditionOnInput_ShouldTransformOrNot(bool whenResult, string expected)
        {
            var result = "any".TeeWhen(s => "teezed", s => whenResult);

            result.Should().Be(expected);
        }

        [Test]
        public void OrElse_ShouldExtractOptionValueOrElse()
        {
            var value = Some(10);

            var result = value.OrElse(0);

            result.Should().Be(10);
        }

        [Test]
        public void OrElse_ShouldExtractOtherValueOrElse()
        {
            Option<int> value = None;

            var result = value.OrElse(0);

            result.Should().Be(0);
        }

        [Test]
        public void ToOptionAsync_WithNull_ShouldBeNone()
        {
            var canBeDispose = Task.FromResult((CanBeDisposed)null);

            var option = canBeDispose.ToOptionAsync().Result;

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsync_WithNull_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult((CanBeDisposed)null);

            var either = canBeDispose.MakeEitherAsync(10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherAsyncLeftFunc_WithNull_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult((CanBeDisposed)null);

            var either = canBeDispose.MakeEitherAsync(() => 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOptionAsync_WithValueAndNoneFunction_ShouldBeNone()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var option = canBeDispose.ToOptionAsync(_ => true).Result;

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsync_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(_ => true, 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherAsyncLeftFunc_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(_ => true, () => 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOptionAsync_WithValue_ShouldBeSome()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var option = canBeDispose.ToOptionAsync().Result;

            option.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsync_WithValue_ShouldBeRight()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(10).Result;

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncLeftFunc_WithValue_ShouldBeRight()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(() => 10).Result;

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void ToOptionAsyncMapping_WithNull_ShouldBeNone()
        {
            var canBeDispose = Task.FromResult((CanBeDisposed)null);

            var option = canBeDispose.ToOptionAsync(_ => 1, _ => true).Result;

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncMapping_WithNull_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult((CanBeDisposed)null);

            var either = canBeDispose.MakeEitherAsync(_ => 1, _ => true, 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherAsyncLeftFuncMapping_WithNull_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult((CanBeDisposed)null);

            var either = canBeDispose.MakeEitherAsync(_ => 1, _ => true, () => 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOptionAsyncMapping_WithBool_ShouldHandleProperly()
        {
            var optionSome = Task.FromResult(false).ToOptionAsync(_ => _).Result;
            var optionNone = Task.FromResult(true).ToOptionAsync(_ => _).Result;

            optionNone.IsNone.Should().BeTrue();
            optionNone.IsSome.Should().BeFalse();

            optionSome.IsNone.Should().BeFalse();
            optionSome.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncMapping_WithBool_ShouldHandleProperly()
        {
            var eitherRight = Task.FromResult(false).MakeEitherAsync(_ => _, 10).Result;
            var eitherLeft = Task.FromResult(true).MakeEitherAsync(_ => _, 10).Result;

            eitherLeft.IsLeft.Should().BeTrue();
            eitherLeft.IsRight.Should().BeFalse();

            eitherRight.IsLeft.Should().BeFalse();
            eitherRight.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncLeftFuncMapping_WithBool_ShouldHandleProperly()
        {
            var eitherRight = Task.FromResult(false).MakeEitherAsync(_ => _, () => 10).Result;
            var eitherLeft = Task.FromResult(true).MakeEitherAsync(_ => _, () => 10).Result;

            eitherLeft.IsLeft.Should().BeTrue();
            eitherLeft.IsRight.Should().BeFalse();

            eitherRight.IsLeft.Should().BeFalse();
            eitherRight.IsRight.Should().BeTrue();
        }

        [Test]
        public void ToOptionAsyncMapping_WithValueType_ShouldHandleProperly()
        {
            var value = Task.FromResult(new ValueTypeStruct { Success = true });

            var option = value.ToOptionAsync(_ => _.Success == false).Result;

            option.IsNone.Should().BeFalse();
        }

        [Test]
        public void MakeEitherAsyncMapping_WithValueType_ShouldHandleProperly()
        {
            var value = Task.FromResult(new ValueTypeStruct { Success = true });

            var either = value.MakeEitherAsync(_ => _.Success == false, 10).Result;

            either.IsLeft.Should().BeFalse();
        }

        [Test]
        public void MakeEitherAsyncLeftFuncMapping_WithValueType_ShouldHandleProperly()
        {
            var value = Task.FromResult(new ValueTypeStruct { Success = true });

            var either = value.MakeEitherAsync(_ => _.Success == false, () => 10).Result;

            either.IsLeft.Should().BeFalse();
        }

        [Test]
        public void ToOptionAsyncMapping_WithDefaultValueType_ShouldHandleProperly()
        {
            var value = Task.FromResult(new ValueTypeStruct());

            var option = value.ToOptionAsync(_ => _.Success).Result;

            option.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncMapping_WithDefaultValueType_ShouldHandleProperly()
        {
            var value = Task.FromResult(new ValueTypeStruct());

            var either = value.MakeEitherAsync(_ => _.Success, 10).Result;

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncLeftFuncMapping_WithDefaultValueType_ShouldHandleProperly()
        {
            var value = Task.FromResult(new ValueTypeStruct());

            var either = value.MakeEitherAsync(_ => _.Success, () => 10).Result;

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void ToOptionAsyncMapping_WithValueAndNoneFunction_ShouldBeNone()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var option = canBeDispose.ToOptionAsync(_ => 1, _ => true).Result;

            option.IsNone.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncMapping_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(_ => 1, _ => true, 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void MakeEitherAsyncLeftFuncMapping_WithValueAndNoneFunction_ShouldBeLeft()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(_ => 1, _ => true, () => 10).Result;

            either
                .IsLeft
                .Should()
                .BeTrue();

            either
                .IfLeft(_ => _.Should().Be(10));
        }

        [Test]
        public void ToOptionAsyncMapping_WithValue_ShouldBeSome()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var option = canBeDispose.ToOptionAsync(_ => 1, _ => false).Result;

            option.IsSome.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncMapping_WithValue_ShouldBeRight()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(_ => 1, _ => false, 10).Result;

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MakeEitherAsyncLeftFuncMapping_WithValue_ShouldBeRight()
        {
            var canBeDispose = Task.FromResult(new CanBeDisposed(new Mock<ILog>().Object));

            var either = canBeDispose.MakeEitherAsync(_ => 1, _ => false, () => 10).Result;

            either.IsRight.Should().BeTrue();
        }

        [Test]
        public void MapAsync_TaskSourceFunc_ShouldCall()
        {
            var source = 0;

            var result = source.MapAsync(async _ => await Task.FromResult(1)).Result;

            result.Should().Be(1);
        }

        [Test]
        public void MapAsync_TaskSourceFunc_NoAwait_ShouldCall()
        {
            var source = 0;

            var result = source.MapAsync(_ => Task.FromResult(1)).Result;

            result.Should().Be(1);
        }

        [Test]
        public void MapAsync_TaskSourceTaskFunc_NoAwait_ShouldCall()
        {
            var canBeRemap = Task.FromResult(0);

            var result = canBeRemap.MapAsync(_ => 1).Result;

            result.Should().Be(1);
        }

        [Test]
        public void Map_WithValue_ShouldMapToResultValue()
        {
            var source = 10;

            var result = source.Map(@this => @this.ToString());

            result.Should().Be("10");
        }

        [Test]
        public void SameMap_WithValue_ShouldMapToResultValue()
        {
            var source = (10, 37);

            var result = source.SameMap(tuple => tuple.ToString());

            result.Should().Be(("10", "37"));
        }

        [Test]
        public void Map_WithOptionValue_ShouldMapToResultValue()
        {
            var some = Option<int>.Some(10);
            var none = Option<int>.None;

            var resultSome = some.Map((Option<int> @this) => @this.ToString());
            var resultNone = none.Map((Option<int> @this) => @this.ToString());

            resultSome.Should().Be("Some(10)");
            resultNone.Should().Be("None");
        }

        [Test]
        public void Map_WithEitherValue_ShouldMapToResultValue()
        {
            var right = (Either<string, int>)10;
            var left = (Either<string, int>)"ERROR";

            var resultRight = right.Map((Either<string, int> @this) => @this.ToString());
            var resultLeft = left.Map((Either<string, int> @this) => @this.ToString());

            resultRight.Should().Be("Right(10)");
            resultLeft.Should().Be("Left(ERROR)");
        }

        [Test]
        public void EitherMapLeftAsync_WhenRight_ShouldNotCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)"Hello world!");

            var result = either.MapLeftAsync(intLeft => Task.FromResult(intLeft * 10)).Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello world!"));
        }

        [Test]
        public void EitherMapLeftAsync_WhenLeft_ShouldCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either.MapLeftAsync(intLeft => Task.FromResult(intLeft * 10)).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result
                .IfLeft(_ => _.Should().Be(100));
        }

        [Test]
        public void EitherMatchAsync_WhenRight_ShouldCallTheRightFunc()
        {
            var either = Task.FromResult((Either<int, string>)"Hello");

            var result = either
                            .MatchAsync(
                                strRight => (Either<int, string>)$"{strRight} world!",
                                intLeft => intLeft * 10
                            ).Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello world!"));
        }

        [Test]
        public void EitherMatchAsync_WhenLeft_ShouldCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either
                            .MatchAsync(
                                strRight => (Either<int, string>)$"{strRight} world!",
                                intLeft => intLeft * 10
                            ).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result
                .IfLeft(_ => _.Should().Be(100));
        }

        [Test]
        public void EitherMatchAsync_WhenRightWithRightTaskFunc_ShouldCallTheRightFunc()
        {
            var either = Task.FromResult((Either<int, string>)"Hello");

            var result = either
                            .MatchAsync(
                                strRight => Task.FromResult((Either<int, string>)$"{strRight} world!"),
                                intLeft => intLeft * 10
                            ).Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello world!"));
        }

        [Test]
        public void EitherMatchAsync_WhenLeftWithRightTaskFunc_ShouldCallTheRightFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either
                            .MatchAsync(
                                strRight => Task.FromResult((Either<int, string>)$"{strRight} world!"),
                                intLeft => intLeft * 10
                            ).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result
                .IfLeft(_ => _.Should().Be(100));
        }

        [Test]
        public void EitherMatchAsync_WhenRightWithLeftTaskFunc_ShouldCallTheRightFunc()
        {
            var either = Task.FromResult((Either<int, string>)"Hello");

            var result = either
                            .MatchAsync(
                                strRight => $"{strRight} world!",
                                intLeft => Task.FromResult((Either<int, string>)(intLeft * 10))
                            ).Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello world!"));
        }

        [Test]
        public void EitherMatchAsync_WhenLeftWithLeftTaskFunc_ShouldCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either
                            .MatchAsync(
                                strRight => $"{strRight} world!",
                                intLeft => Task.FromResult((Either<int, string>)(intLeft * 10))
                            ).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result
                .IfLeft(_ => _.Should().Be(100));
        }

        [Test]
        public void EitherMatchAsync_WhenRightWithBothTaskFunc_ShouldCallTheRightFunc()
        {
            var either = Task.FromResult((Either<int, string>)"Hello");

            var result = either
                            .MatchAsync(
                                strRight => Task.FromResult((Either<int, string>)$"{strRight} world!"),
                                intLeft => Task.FromResult((Either<int, string>)(intLeft * 10))
                            ).Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello world!"));
        }

        [Test]
        public void EitherMatchAsync_WhenLeftWithBothTaskFunc_ShouldCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either
                            .MatchAsync(
                                strRight => Task.FromResult((Either<int, string>)$"{strRight} world!"),
                                intLeft => Task.FromResult((Either<int, string>)(intLeft * 10))
                            ).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result
                .IfLeft(_ => _.Should().Be(100));
        }

        [Test]
        public void BindLeftAsync_WhenRight_ShouldNotCallTheLeftFunc()
        {
            var result = Task.FromResult((Either<int, string>)"Hello")
                        .BindLeftAsync(intLeft => (Either<int, string>)(intLeft * 10))
                        .Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello"));
        }

        [Test]
        public void BindLeftAsync_WhenLeft_ShouldCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either.BindLeftAsync(intLeft => (Either<int, string>)(intLeft * 10)).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result.IfLeft(_ => _.Should().Be(100));
        }

        [Test]
        public void MapLeftAsync_WhenRight_ShouldNotCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)"Hello");

            var result = either.MapLeftAsync(intLeft => intLeft * 10).Result;

            result
                .IsRight
                .Should()
                .BeTrue();

            result
                .IfRight(_ => _.Should().Be("Hello"));
        }

        [Test]
        public void MapLeftAsync_WhenLeft_ShouldCallTheLeftFunc()
        {
            var either = Task.FromResult((Either<int, string>)10);

            var result = either.MapLeftAsync(intLeft => intLeft * 10).Result;

            result
                .IsLeft
                .Should()
                .BeTrue();

            result
                .IfLeft(_ => _.Should().Be(100));
        }
    }
}