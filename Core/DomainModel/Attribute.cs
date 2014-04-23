using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FIVESServiceBus;

namespace FIVES
{
    public class Attribute
    {
        public Attribute(ReadOnlyAttributeDefinition definition, Component parentComponent)
        {
            ParentComponent = parentComponent;
            Definition = definition;
            Value = definition.DefaultValue;
        }

        public Component ParentComponent { get; private set; }

        public object Value
        {
            get
            {
                return CurrentValue;
            }
            
            internal set 
            {
                Set(value);
            }
        }

        public Type Type
        {
            get { return Definition.Type; }
        }

        public void Suggest(object newValue)
        {
            if (World.Instance.ContainsEntity(ParentComponent.ContainingEntity.Guid))
            {
                var proposedChange =
                    new ProposeAttributeChangeEventArgs(ParentComponent.ContainingEntity,
                                                        ParentComponent.Name,
                                                        Definition.Name,
                                                        newValue);

                ParentComponent.ContainingEntity.PublishAttributeChangeSuggestion(proposedChange);
            }
            else
            {
                Set(newValue);
            }
        }

        public T As<T>()
        {
            return (T)Value;
        }

        internal void Set(object value)
        {
            if ((value == null && !CanBeAssignedNull(Type))
                || (value != null && !Type.IsAssignableFrom(value.GetType())))
            {
                throw new AttributeAssignmentException("Attribute can not be assigned from provided value.");
            }

            var oldValue = CurrentValue;
            CurrentValue = value;
            ParentComponent.raiseChangeEvent(Definition.Name, oldValue, CurrentValue);
        }
            
        private static bool CanBeAssignedNull(Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        private ReadOnlyAttributeDefinition Definition;
        private object CurrentValue;
    }
}
