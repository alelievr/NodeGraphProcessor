using System;

public partial class DelegateEvent<T>
{
    public class VarObjectEventArgs : EventArgs
    {
        private T value;
        public T Value => value;

        public VarObjectEventArgs(T value)
        {
            this.value = value;
        }
    }
}

