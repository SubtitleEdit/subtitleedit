using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaloniaEdit
{
    public class RoutedCommand : ICommand
    {
        private static IInputElement _inputElement;

        public string Name { get; }
        public KeyGesture Gesture { get; }

        public RoutedCommand(string name, KeyGesture keyGesture = null)
        {
            Name = name;
            Gesture = keyGesture;
        }

        static RoutedCommand()
        {
            CanExecuteEvent.AddClassHandler<Interactive>(CanExecuteEventHandler);
            ExecutedEvent.AddClassHandler<Interactive>(ExecutedEventHandler);
            InputElement.GotFocusEvent.AddClassHandler<Interactive>(GotFocusEventHandler);
        }

        private static void CanExecuteEventHandler(Interactive control, CanExecuteRoutedEventArgs args)
        {
            if (control is IRoutedCommandBindable bindable)
            {
                var binding = bindable.CommandBindings.Where(c => c != null)
               .FirstOrDefault(c => c.Command == args.Command && c.DoCanExecute(control, args));
                args.CanExecute = binding != null;
            }
        }

        private static void ExecutedEventHandler(Interactive control, ExecutedRoutedEventArgs args)
        {
            if (control is IRoutedCommandBindable bindable)
            {
                // ReSharper disable once UnusedVariable
                var binding = bindable.CommandBindings.Where(c => c != null)
                    .FirstOrDefault(c => c.Command == args.Command && c.DoExecuted(control, args));

            }
        }

        private static void GotFocusEventHandler(Interactive control, FocusChangedEventArgs args)
        {
            _inputElement = args.Source as IInputElement;
        }

        public static RoutedEvent<CanExecuteRoutedEventArgs> CanExecuteEvent { get; } = RoutedEvent.Register<CanExecuteRoutedEventArgs>(nameof(CanExecuteEvent), RoutingStrategies.Bubble, typeof(RoutedCommand));

        public bool CanExecute(object parameter, IInputElement target)
        {
            if (target == null) return false;

            var args = new CanExecuteRoutedEventArgs(this, parameter);
            target.RaiseEvent(args);

            return args.CanExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter, _inputElement);
        }

        public static RoutedEvent<ExecutedRoutedEventArgs> ExecutedEvent { get; } = RoutedEvent.Register<ExecutedRoutedEventArgs>(nameof(ExecutedEvent), RoutingStrategies.Bubble, typeof(RoutedCommand));

        public void Execute(object parameter, IInputElement target)
        {
            if (target == null) return;

            var args = new ExecutedRoutedEventArgs(this, parameter);
            target.RaiseEvent(args);
        }

        void ICommand.Execute(object parameter)
        {
            Execute(parameter, _inputElement);
        }

        // TODO
        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    public interface IRoutedCommandBindable
    {
        IList<RoutedCommandBinding> CommandBindings { get; }
    }

    public class RoutedCommandBinding
    {
        public RoutedCommandBinding(RoutedCommand command,
            EventHandler<ExecutedRoutedEventArgs> executed = null,
            EventHandler<CanExecuteRoutedEventArgs> canExecute = null)
        {
            Command = command;
            if (executed != null) Executed += executed;
            if (canExecute != null) CanExecute += canExecute;
        }

        public RoutedCommand Command { get; }

        public event EventHandler<CanExecuteRoutedEventArgs> CanExecute;

        public event EventHandler<ExecutedRoutedEventArgs> Executed;

        internal bool DoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Handled) return true;

            var canExecute = CanExecute;
            if (canExecute == null)
            {
                if (Executed != null)
                {
                    e.Handled = true;
                    e.CanExecute = true;
                }
            }
            else
            {
                canExecute(sender, e);

                if (e.CanExecute)
                {
                    e.Handled = true;
                }
            }

            return e.CanExecute;
        }

        internal bool DoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                var executed = Executed;

                if (executed != null)
                {
                    if (DoCanExecute(sender, new CanExecuteRoutedEventArgs(e.Command, e.Parameter)))
                    {
                        executed(sender, e);
                        e.Handled = true;
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public sealed class CanExecuteRoutedEventArgs : RoutedEventArgs
    {
        public ICommand Command { get; }

        public object Parameter { get; }

        public bool CanExecute { get; set; }

        internal CanExecuteRoutedEventArgs(ICommand command, object parameter)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Parameter = parameter;
            RoutedEvent = RoutedCommand.CanExecuteEvent;
        }
    }

    public sealed class ExecutedRoutedEventArgs : RoutedEventArgs
    {
        public ICommand Command { get; }

        public object Parameter { get; }

        internal ExecutedRoutedEventArgs(ICommand command, object parameter)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Parameter = parameter;
            RoutedEvent = RoutedCommand.ExecutedEvent;
        }
    }
}