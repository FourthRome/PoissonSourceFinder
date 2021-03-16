using System;

namespace Contracts
{
    public class ModelEventArgs<T> : EventArgs where T: IPoint
    {
        public ModelEventArgs(string message, ISourceGroup<T> group=null)
        {
            Message = message;
            Group = group;
        }

        public string Message { get; }
        public ISourceGroup<T> Group { get; }
    }
}
