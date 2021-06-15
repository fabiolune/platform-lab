using System;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Common.Extensions
{
    public static class FunctionalExtension
    {
        public static T TeeWhen<T>(this T @this, Func<T, T> tee, Func<T, bool> when)
            => when(@this) ? tee(@this) : @this;

        public static T TeeWhen<T>(this T @this, Func<T, T> tee, Func<bool> when)
            => when() ? tee(@this) : @this;

        public static T Tee<T>(this T @this, Func<T, T> tee)
            => tee(@this);

        public static T Tee<T>(this T @this, Action<T> tee)
            => @this.Tee(_ =>
            {
                tee(_);
                return _;
            });

        public static void Do<T>(this T @this, Action<T> action)
            => action(@this);

        public static Unit Using<TD>(TD disposable, Action<TD> action)
            where TD : IDisposable
        {
            using (disposable)
                action(disposable);
            return Unit.Default;
        }

        public static T Using<TD, T>(TD disposable, Func<TD, T> func)
            where TD : IDisposable
        {
            using (disposable)
                return func(disposable);
        }

        public static Unit Using<TD1, TD2>(TD1 disposable1, Func<TD1, TD2> createDisposable2, Action<TD1, TD2> action)
            where TD1 : IDisposable
            where TD2 : IDisposable
        {
            using (disposable1)
            using (var disposable2 = createDisposable2(disposable1))
                action(disposable1, disposable2);
            return Unit.Default;
        }

        public static T Using<TD1, TD2, T>(TD1 disposable1, Func<TD1, TD2> createDisposable2, Func<TD1, TD2, T> func)
            where TD1 : IDisposable
            where TD2 : IDisposable
        {
            using (disposable1)
            using (var disposable2 = createDisposable2(disposable1))
                return func(disposable1, disposable2);
        }

        public static async Task<Unit> UsingAsync<TD>(TD disposable, Func<TD, Task> action)
            where TD : IDisposable
        {
            using (disposable)
                await action(disposable);
            return Unit.Default;
        }

        public static async Task<T> UsingAsync<TD, T>(TD disposable, Func<TD, Task<T>> func)
            where TD : IDisposable
        {
            using (disposable)
                return await func(disposable);
        }

        public static async Task<Unit> UsingAsync<TD1, TD2>(TD1 disposable1, Func<TD1, TD2> createDisposable2, Func<TD1, TD2, Task> action)
            where TD1 : IDisposable
            where TD2 : IDisposable
        {
            using (disposable1)
            using (var disposable2 = createDisposable2(disposable1))
                await action(disposable1, disposable2);
            return Unit.Default;
        }

        public static async Task<T> UsingAsync<TD1, TD2, T>(TD1 disposable1, Func<TD1, TD2> createDisposable2, Func<TD1, TD2, Task<T>> func)
            where TD1 : IDisposable
            where TD2 : IDisposable
        {
            using (disposable1)
            using (var disposable2 = createDisposable2(disposable1))
                return await func(disposable1, disposable2);
        }

        public static T OrElse<T>(this Option<T> @this, T defaultValue)
            => @this.IfNone(defaultValue);

        public static Option<T> ToOption<T>(this T @this)
            => ToOption(@this, _ => false);

        public static Option<T> ToOption<T>(this T @this, Predicate<T> noneWhen)
            => ToOption(@this, _ => _, noneWhen);

        public static Option<TResult> ToOption<TInput, TResult>(this TInput @this,
                                                                  Func<TInput, TResult> map,
                                                                  Predicate<TInput> noneWhen)
            => @this == null || noneWhen(@this) ? None : Some(map(@this));
        
        public static Task<Option<T>> ToOptionAsync<T>(this Task<T> @this)
            => ToOptionAsync(@this, _ => false);

        public static Task<Option<T>> ToOptionAsync<T>(this Task<T> @this, Predicate<T> noneWhen)
            => ToOptionAsync(@this, _ => _, noneWhen);

        public static async Task<Option<TResult>> ToOptionAsync<TInput, TResult>(this Task<TInput> @this,
                                                                  Func<TInput, TResult> map,
                                                                  Predicate<TInput> noneWhen)
        {
            var value = await @this;
            return value == null || noneWhen(value) ? None : Some(map(value));
        }

        public static TResult Map<TSource, TResult>(this TSource @this, Func<TSource, TResult> fn)
            => fn(@this);

        public static (TResult, TResult) SameMap<TSource, TResult>(this (TSource, TSource) @this,
            Func<TSource, TResult> fn)
            => (fn(@this.Item1), fn(@this.Item2));

        public static async Task<TResult> MapAsync<TSource, TResult>(this Task<TSource> @this,
                                                                     Func<TSource, Task<TResult>> fn)
            => await fn(await @this);

        public static Task<TResult> MapAsync<TSource, TResult>(this TSource @this,
                                                               Func<TSource, Task<TResult>> fn)
            => fn(@this);

        public static async Task<TResult> MapAsync<TSource, TResult>(this Task<TSource> @this,
                                                                     Func<TSource, TResult> fn)
            => fn(await @this);

        public static async Task<Either<TMl, TR>> MapLeftAsync<TL, TR, TMl>(this Task<Either<TL, TR>> @this,
                                                                           Func<TL, Task<TMl>> onLeftAsync)
        {
            var either = await @this;
            return await either
                    .MatchAsync<Either<TMl, TR>>(
                        _ => _,
                        async left => await onLeftAsync(left)
                    );
        }
        public static async Task<Either<TMl, TR>> MapLeftAsync<TL, TR, TMl>(this Task<Either<TL, TR>> @this, Func<TL, TMl> onLeft)
            => (await @this).BindLeft(left => (Either<TMl, TR>)onLeft(left));

        public static async Task<TOutput> MatchAsync<TL, TR, TOutput>(this Task<Either<TL, TR>> @this,
                                                                Func<TR, TOutput> onRight,
                                                                Func<TL, TOutput> onLeft)
            => (await @this)
                        .Match(
                            onRight,
                            onLeft
                        );

        public static async Task<TOutput> MatchUnsafeAsync<TL, TR, TOutput>(this Task<Either<TL, TR>> @this,
                                                                Func<TR, TOutput> onRight,
                                                                Func<TL, TOutput> onLeft)
            => (await @this)
                        .MatchUnsafe(
                            onRight,
                            onLeft
                        );

        public static Task<TOutput> MatchAsync<TL, TR, TOutput>(this Task<Either<TL, TR>> @this,
                                                            Func<TR, Task<TOutput>> onRightAsync,
                                                            Func<TL, Task<TOutput>> onLeftAsync)
            => @this
                .MapAsync(onRightAsync)
                .MapLeftAsync(onLeftAsync)
                .MatchAsync(
                    _ => _,
                    _ => _
                );

        public static Task<TOutput> MatchAsync<TL, TR, TOutput>(this Task<Either<TL, TR>> @this,
                                                            Func<TR, Task<TOutput>> onRightAsync,
                                                            Func<TL, TOutput> onLeft)
            => @this
                .MapAsync(onRightAsync)
                .MatchAsync(
                    _ => _,
                    onLeft
                );

        public static Task<TOutput> MatchAsync<TL, TR, TOutput>(this Task<Either<TL, TR>> @this,
                                                            Func<TR, TOutput> onRight,
                                                            Func<TL, Task<TOutput>> onLeftAsync)
            => @this
                .MapLeftAsync(onLeftAsync)
                .MatchAsync(
                    onRight,
                    _ => _
                );

        public static Either<TL, TR> MakeEither<TL, TR>(this TR @this, TL leftValue)
            => ToOption(@this, _ => false)
                .ToEither(leftValue);

        public static Either<TL, TR> MakeEither<TL, TR>(this TR @this, Predicate<TR> leftWhen, TL leftValue)
            => ToOption(@this, _ => _, leftWhen)
                .ToEither(leftValue);

        public static Either<TL, TROutput> MakeEither<TRInput, TROutput, TL>(this TRInput @this,
                                                                  Func<TRInput, TROutput> map,
                                                                  Predicate<TRInput> leftWhen,
                                                                  TL leftValue)
            => ToOption(@this, map, leftWhen)
                .ToEither(leftValue);

        public static Either<TL, TR> MakeEither<TL, TR>(this TR @this, Func<TL> leftFunc)
            => ToOption(@this, _ => false)
                .ToEither(leftFunc);

        public static Either<TL, TR> MakeEither<TL, TR>(this TR @this, Predicate<TR> leftWhen, Func<TL> leftFunc)
            => ToOption(@this, _ => _, leftWhen)
                .ToEither(leftFunc);

        public static Either<TL, TROutput> MakeEither<TRInput, TROutput, TL>(this TRInput @this,
                                                                  Func<TRInput, TROutput> map,
                                                                  Predicate<TRInput> leftWhen,
                                                                  Func<TL> leftFunc)
            => ToOption(@this, map, leftWhen)
                .ToEither(leftFunc);

        public static Task<Either<TL, TR>> MakeEitherAsync<TL, TR>(this Task<TR> @this, TL leftValue)
            => MakeEitherAsync(@this, _ => false, leftValue);

        public static Task<Either<TL, TR>> MakeEitherAsync<TL, TR>(this Task<TR> @this, Predicate<TR> leftWhen, TL leftValue)
            => MakeEitherAsync(@this, _ => _, leftWhen, leftValue);

        public static async Task<Either<TL, TROutput>> MakeEitherAsync<TL, TRInput, TROutput>(this Task<TRInput> @this,
                                                                  Func<TRInput, TROutput> map,
                                                                  Predicate<TRInput> leftWhen,
                                                                  TL leftValue)
            => (await @this.ToOptionAsync(map, leftWhen))
                .ToEither(leftValue);

        public static Task<Either<TL, TR>> MakeEitherAsync<TL, TR>(this Task<TR> @this, Func<TL> leftFunc)
            => MakeEitherAsync(@this, _ => false, leftFunc);

        public static Task<Either<TL, TR>> MakeEitherAsync<TL, TR>(this Task<TR> @this, Predicate<TR> leftWhen, Func<TL> leftFunc)
            => MakeEitherAsync(@this, _ => _, leftWhen, leftFunc);

        public static async Task<Either<TL, TROutput>> MakeEitherAsync<TL, TRInput, TROutput>(this Task<TRInput> @this,
                                                                  Func<TRInput, TROutput> map,
                                                                  Predicate<TRInput> leftWhen,
                                                                  Func<TL> leftFunc)
            => (await @this.ToOptionAsync(map, leftWhen))
                .ToEither(leftFunc);

        public static async Task<Either<TMl, TR>> BindLeftAsync<TL, TR, TMl>(this Task<Either<TL, TR>> @this, Func<TL, Either<TMl, TR>> onLeft)
            => (await @this).BindLeft(onLeft);

    }
}