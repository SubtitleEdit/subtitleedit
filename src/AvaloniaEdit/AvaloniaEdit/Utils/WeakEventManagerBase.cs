using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AvaloniaEdit.Utils
{
    /// <summary>
    /// WeakEventManager base class. Inspired by the WPF WeakEventManager class and the code in 
    /// https://social.msdn.microsoft.com/Forums/silverlight/en-US/34d85c3f-52ea-4adc-bb32-8297f5549042/command-binding-memory-leak?forum=silverlightbugs
    /// </summary>
    /// <remarks>Copied here from ReactiveUI due to bugs in its design (singleton instance for multiple events).</remarks>
    /// <typeparam name="TEventManager"></typeparam>
    /// <typeparam name="TEventSource">The type of the event source.</typeparam>
    /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    public abstract class WeakEventManagerBase<TEventManager, TEventSource, TEventHandler, TEventArgs>
        where TEventManager : WeakEventManagerBase<TEventManager, TEventSource, TEventHandler, TEventArgs>, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object StaticSource = new object();

        /// <summary>
        /// Mapping between the target of the delegate (for example a Button) and the handler (EventHandler).
        /// Windows Phone needs this, otherwise the event handler gets garbage collected.
        /// </summary>
        private readonly ConditionalWeakTable<object, List<Delegate>> _targetToEventHandler = new ConditionalWeakTable<object, List<Delegate>>();

        /// <summary>
        /// Mapping from the source of the event to the list of handlers. This is a CWT to ensure it does not leak the source of the event.
        /// </summary>
        private readonly ConditionalWeakTable<object, WeakHandlerList> _sourceToWeakHandlers = new ConditionalWeakTable<object, WeakHandlerList>();

        private static readonly Lazy<TEventManager> CurrentLazy = new Lazy<TEventManager>(() => new TEventManager());

        private static TEventManager Current => CurrentLazy.Value;

        /// <summary>
        /// Adds a weak reference to the handler and associates it with the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler.</param>
        public static void AddHandler(TEventSource source, TEventHandler handler)
        {
            Current.PrivateAddHandler(source, handler);
        }

        /// <summary>
        /// Removes the association between the source and the handler.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="handler">The handler.</param>
        public static void RemoveHandler(TEventSource source, TEventHandler handler)
        {
            Current.PrivateRemoveHandler(source, handler);
        }

        /// <summary>
        /// Delivers the event to the handlers registered for the source. 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="TEventArgs"/> instance containing the event data.</param>
        protected static void DeliverEvent(object sender, TEventArgs args)
        {
            Current.PrivateDeliverEvent(sender, args);
        }

        /// <summary>
        /// Override this method to attach to an event.
        /// </summary>
        /// <param name="source">The source.</param>
        protected abstract void StartListening(TEventSource source);

        /// <summary>
        /// Override this method to detach from an event.
        /// </summary>
        /// <param name="source">The source.</param>
        protected abstract void StopListening(TEventSource source);

        protected void PrivateAddHandler(TEventSource source, TEventHandler handler)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!typeof(TEventHandler).GetTypeInfo().IsSubclassOf(typeof(Delegate)))
            {
                throw new ArgumentException("Handler must be Delegate type");
            }

            AddWeakHandler(source, handler);
            AddTargetHandler(handler);
        }

        private void AddWeakHandler(TEventSource source, TEventHandler handler)
        {
            if (_sourceToWeakHandlers.TryGetValue(source, out var weakHandlers))
            {
                // clone list if we are currently delivering an event
                if (weakHandlers.IsDeliverActive)
                {
                    weakHandlers = weakHandlers.Clone();
                    _sourceToWeakHandlers.Remove(source);
                    _sourceToWeakHandlers.Add(source, weakHandlers);
                }
                weakHandlers.AddWeakHandler(source, handler);
            }
            else
            {
                weakHandlers = new WeakHandlerList();
                weakHandlers.AddWeakHandler(source, handler);

                _sourceToWeakHandlers.Add(source, weakHandlers);
                StartListening(source);
            }

            Purge(source);
        }

        private void AddTargetHandler(TEventHandler handler)
        {
            var @delegate = handler as Delegate;
            var key = @delegate?.Target ?? StaticSource;

            if (_targetToEventHandler.TryGetValue(key, out var delegates))
            {
                delegates.Add(@delegate);
            }
            else
            {
                delegates = new List<Delegate> { @delegate };

                _targetToEventHandler.Add(key, delegates);
            }
        }

        protected void PrivateRemoveHandler(TEventSource source, TEventHandler handler)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!typeof(TEventHandler).GetTypeInfo().IsSubclassOf(typeof(Delegate)))
            {
                throw new ArgumentException("handler must be Delegate type");
            }

            RemoveWeakHandler(source, handler);
            RemoveTargetHandler(handler);
        }

        private void RemoveWeakHandler(TEventSource source, TEventHandler handler)
        {
            if (_sourceToWeakHandlers.TryGetValue(source, out var weakHandlers))
            {
                // clone list if we are currently delivering an event
                if (weakHandlers.IsDeliverActive)
                {
                    weakHandlers = weakHandlers.Clone();
                    _sourceToWeakHandlers.Remove(source);
                    _sourceToWeakHandlers.Add(source, weakHandlers);
                }

                if (weakHandlers.RemoveWeakHandler(source, handler) && weakHandlers.Count == 0)
                {
                    _sourceToWeakHandlers.Remove(source);
                    StopListening(source);
                }
            }
        }

        private void RemoveTargetHandler(TEventHandler handler)
        {
            var @delegate = handler as Delegate;
            var key = @delegate?.Target ?? StaticSource;

            if (_targetToEventHandler.TryGetValue(key, out var delegates))
            {
                delegates.Remove(@delegate);

                if (delegates.Count == 0)
                {
                    _targetToEventHandler.Remove(key);
                }
            }
        }

        private void PrivateDeliverEvent(object sender, TEventArgs args)
        {
            var source = sender ?? StaticSource;

            var hasStaleEntries = false;

            if (_sourceToWeakHandlers.TryGetValue(source, out var weakHandlers))
            {
                using (weakHandlers.DeliverActive())
                {
                    hasStaleEntries = weakHandlers.DeliverEvent(source, args);
                }
            }

            if (hasStaleEntries)
            {
                Purge(source);
            }
        }

        private void Purge(object source)
        {
            if (_sourceToWeakHandlers.TryGetValue(source, out var weakHandlers))
            {
                if (weakHandlers.IsDeliverActive)
                {
                    weakHandlers = weakHandlers.Clone();
                    _sourceToWeakHandlers.Remove(source);
                    _sourceToWeakHandlers.Add(source, weakHandlers);
                }
                else
                {
                    weakHandlers.Purge();
                }
            }
        }

        internal class WeakHandler
        {
            private readonly WeakReference _source;
            private readonly WeakReference _originalHandler;

            public bool IsActive => _source != null && _source.IsAlive && _originalHandler != null && _originalHandler.IsAlive;

            public TEventHandler Handler
            {
                get
                {
                    if (_originalHandler == null)
                    {
                        return default(TEventHandler);
                    }
                    return (TEventHandler)_originalHandler.Target;
                }
            }

            public WeakHandler(object source, TEventHandler originalHandler)
            {
                _source = new WeakReference(source);
                _originalHandler = new WeakReference(originalHandler);
            }

            public bool Matches(object o, TEventHandler handler)
            {
                return _source != null &&
                       ReferenceEquals(_source.Target, o) &&
                       _originalHandler != null &&
                       (ReferenceEquals(_originalHandler.Target, handler) ||
                        _originalHandler.Target is TEventHandler &&
                        handler is TEventHandler &&
                        handler is Delegate del && _originalHandler.Target is Delegate origDel && Equals(del.Target, origDel.Target));
            }
        }

        internal class WeakHandlerList
        {
            private int _deliveries;
            private readonly List<WeakHandler> _handlers;

            public WeakHandlerList()
            {
                _handlers = new List<WeakHandler>();
            }

            public void AddWeakHandler(TEventSource source, TEventHandler handler)
            {
                var handlerSink = new WeakHandler(source, handler);
                _handlers.Add(handlerSink);
            }

            public bool RemoveWeakHandler(TEventSource source, TEventHandler handler)
            {
                foreach (var weakHandler in _handlers)
                {
                    if (weakHandler.Matches(source, handler))
                    {
                        return _handlers.Remove(weakHandler);
                    }
                }

                return false;
            }

            public WeakHandlerList Clone()
            {
                var newList = new WeakHandlerList();
                newList._handlers.AddRange(_handlers.Where(h => h.IsActive));

                return newList;
            }

            public int Count => _handlers.Count;

            public bool IsDeliverActive => _deliveries > 0;

            public IDisposable DeliverActive()
            {
                Interlocked.Increment(ref _deliveries);

                return new Disposable(() => Interlocked.Decrement(ref _deliveries));
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public virtual bool DeliverEvent(object sender, TEventArgs args)
            {
                var hasStaleEntries = false;

                foreach (var handler in _handlers)
                {
                    if (handler.IsActive)
                    {
                        var @delegate = handler.Handler as Delegate;
                        @delegate?.DynamicInvoke(sender, args);
                    }
                    else
                    {
                        hasStaleEntries = true;
                    }
                }

                return hasStaleEntries;
            }

            public void Purge()
            {
                for (var i = _handlers.Count - 1; i >= 0; i--)
                {
                    if (!_handlers[i].IsActive)
                    {
                        _handlers.RemoveAt(i);
                    }
                }
            }
        }
    }

    internal sealed class Disposable : IDisposable
    {
        private volatile Action _dispose;
        public Disposable(Action dispose)
        {
            _dispose = dispose;
        }
        public bool IsDisposed => _dispose == null;
        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}