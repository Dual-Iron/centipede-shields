#nullable enable
using System.Collections.Generic;

namespace CFisobs.Core
{
    public interface IContent
    {
        IEnumerable<Registry> GetRegistries();
    }
}