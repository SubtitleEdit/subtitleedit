using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// This class is used to prevent stack overflows by representing a 'busy' flag
    /// that prevents reentrance when another call is running.
    /// However, using a simple 'bool busy' is not thread-safe, so we use a
    /// thread-static BusyManager.
    /// </summary>
    internal static class BusyManager
    {
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "Should always be used with 'var'")]
        public readonly struct BusyLock : IDisposable
        {
            public static readonly BusyLock Failed = new BusyLock(null);

            private readonly List<object> _objectList;

            internal BusyLock(List<object> objectList)
            {
                _objectList = objectList;
            }

            public bool Success => _objectList != null;

            public void Dispose()
            {
                _objectList?.RemoveAt(_objectList.Count - 1);
            }
        }

        [ThreadStatic] private static List<object> _activeObjects;

        public static BusyLock Enter(object obj)
        {
            var activeObjects = _activeObjects ?? (_activeObjects = new List<object>());
            if (activeObjects.Any(t => t == obj))
            {
                return BusyLock.Failed;
            }
            activeObjects.Add(obj);
            return new BusyLock(activeObjects);
        }
    }
}