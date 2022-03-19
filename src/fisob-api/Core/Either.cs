#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CFisobs.Core
{
    public readonly struct Either<L, R>
    {
        readonly byte discriminant;
        readonly L? l;
        readonly R? r;

        public Either(L left)
        {
            discriminant = 1;
            l = left;
            r = default;
        }

        public Either(R right)
        {
            discriminant = 2;
            l = default;
            r = right;
        }

        public L LeftOr(L fallback) => MatchL(out var left) ? left : fallback;
        public R RightOr(R fallback) => MatchR(out var right) ? right : fallback;

        public bool MatchL([MaybeNullWhen(false)] out L value)
        {
            value = l;
            return discriminant == 1;
        }

        public bool MatchR([MaybeNullWhen(false)] out R value)
        {
            value = r;
            return discriminant == 2;
        }

        public override string? ToString()
        {
            return discriminant switch {
                1 => l?.ToString(),
                2 => r?.ToString(),
                _ => throw new InvalidOperationException("Instance of Either is invalid. Do not use `default` when creating Either instances."),
            };
        }

        public override bool Equals(object o)
        {
            if (discriminant == 1 && o is L left) {
                return left.Equals(l);
            }
            if (discriminant == 2 && o is R right) {
                return right.Equals(r);
            }
            return o is Either<L, R> other &&
                   discriminant == other.discriminant &&
                   EqualityComparer<L?>.Default.Equals(l, other.l) &&
                   EqualityComparer<R?>.Default.Equals(r, other.r);
        }

        public override int GetHashCode()
        {
            int hashCode = 1341583068;
            hashCode = hashCode * -1521134295 + discriminant.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<L?>.Default.GetHashCode(l);
            hashCode = hashCode * -1521134295 + EqualityComparer<R?>.Default.GetHashCode(r);
            return hashCode;
        }

        public static implicit operator Either<L, R>(L left) => new(left);
        public static implicit operator Either<L, R>(R right) => new(right);

        public static bool operator ==(Either<L, R> left, Either<L, R> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Either<L, R> left, Either<L, R> right)
        {
            return !left.Equals(right);
        }
    }
}