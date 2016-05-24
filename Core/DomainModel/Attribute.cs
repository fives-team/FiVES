// This file is part of FiVES.
//
// FiVES is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation (LGPL v3)
//
// FiVES is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with FiVES.  If not, see <http://www.gnu.org/licenses/>.
using SINFONI;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.Specialized;

[assembly: InternalsVisibleTo("ServiceBus")]

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
            if(newValue == null && !CanBeAssignedNull(this.Type))
                    throw(new AttributeAssignmentException("Attribute of type " + this.Type
                    + " can not be assigned from provided value" + newValue + " of type " + newValue.GetType()) );
            try
            {
                var convertedValue = convertValueToAttributeType(newValue);
                if (World.Instance.ContainsEntity(ParentComponent.ContainingEntity.Guid))
                {
                    var proposedChange =
                        new ProposeAttributeChangeEventArgs(ParentComponent.ContainingEntity,
                                                            ParentComponent.Name,
                                                            Definition.Name,
                                                            convertedValue);

                    ParentComponent.ContainingEntity.PublishAttributeChangeSuggestion(proposedChange);
                }
                else
                {
                    Set(convertedValue);
                }
            }
            catch
            {
                throw new AttributeAssignmentException("Attribute of type " + this.Type
                    + " can not be assigned from provided value" + newValue + " of type " + newValue.GetType());
            }
        }

        private object convertValueToAttributeType(object value)
        {
            if (value == null || value.GetType() == this.Type)
                return value;

            if (World.Instance.SinTd != null && World.Instance.SinTd.ContainsType(this.Type.Name))
            {
                SinTDType typeAsKtdType = World.Instance.SinTd.GetSinTDType(this.Type.Name);
                return typeAsKtdType.AssignValuesToNativeType(value, this.Type);
            }

            return Convert.ChangeType(value, this.Type);
        }

        public T As<T>()
        {
            if (!CurrentValue.GetType().IsAssignableFrom(typeof(T)) && World.Instance.SinTd.ContainsType(Type.Name))
            {
                SinTDType typeAsKtdType = World.Instance.SinTd.GetSinTDType(Type.Name);
                Type t = typeof(T);
                return (T)typeAsKtdType.AssignValuesToNativeType(Value, t);
            }
            else
            {
                return (T)Value;
            }
        }

        internal void Set(object value)
        {
            if (value!= null && CurrentValue!= null && !value.Equals(CurrentValue))
            {
                deRegisterEventHandler();
            }
            var oldValue = CurrentValue;
            CurrentValue = value;
            registerChangedEventHandlers();
            ParentComponent.raiseChangeEvent(Definition.Name, oldValue, CurrentValue);
        }

        private void deRegisterEventHandler()
        {
            if (Definition.HasNotifyCollectionChangedNotification)
            {
                ( (INotifyCollectionChanged)Value ).CollectionChanged -= OnCollectionChanged;
            }
            else if (Definition.HasPropertyChangedNotification)
            {
                ( (INotifyPropertyChanged)Value ).PropertyChanged -= OnPropertyChanged;
            }
        }

        private void registerChangedEventHandlers()
        {
            if (Definition.HasNotifyCollectionChangedNotification)
            {
                ( (INotifyCollectionChanged)Value ).CollectionChanged += OnCollectionChanged;
            }
            else if (Definition.HasPropertyChangedNotification)
            {
                ( (INotifyPropertyChanged)Value ).PropertyChanged += OnPropertyChanged;
            }
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            ParentComponent.raiseChangeEventFromInternalChange(Definition.Name, CurrentValue);
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            ParentComponent.raiseChangeEventFromInternalChange(Definition.Name, CurrentValue);
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
