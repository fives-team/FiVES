using System;

namespace Events
{
    public class ValueChangedEventArgs
    {
        public ValueChangedEventArgs (object oldValue, object newValue)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public object oldValue;
        public object newValue;
    }
}

