#nullable enable
using System;

namespace CFisobs.Core
{
    public sealed class RegisterException : Exception
    {
        private RegisterException(string msg) : base(msg) { }

        public static RegisterException InvalidID(string id)
            => new RegisterException($"The ID `{id}` is invalid. Valid IDs are C# identifiers in the ASCII character range.");

        public static RegisterException DuplicateID(string id)
            => new RegisterException($"The ID `{id}` is already taken.");
    }
}