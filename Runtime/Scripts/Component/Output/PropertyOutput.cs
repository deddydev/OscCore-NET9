using System.Numerics;
using System.Reflection;

namespace OscCore
{
    public class PropertyOutput
    {
        OscSender? m_Sender;
        string m_Address = "";
        object? m_SourceObject;
        
        bool m_MemberIsProperty;
        string? m_PropertyName;
        string? m_PropertyTypeName;

        private readonly Vector2ElementFilter m_SendVector2Elements;
        private readonly Vector3ElementFilter m_SendVector3Elements;

        bool m_PreviousBooleanValue;
        int m_PreviousIntValue;
        long m_PreviousLongValue;
        double m_PreviousDoubleValue;
        float m_PreviousSingleValue;
        string m_PreviousStringValue = string.Empty;
        Color32 m_PreviousColorValue;
        Vector2 m_PreviousVec2Value;
        Vector3 m_PreviousVec3Value;

        bool m_HasSender;

        MemberInfo? m_MemberInfo;
        PropertyInfo? m_Property;
        FieldInfo? m_Field;
        
        /// <summary>
        /// The OscCore component that handles serializing and sending messages. Cannot be null
        /// </summary>
        public OscSender? Sender
        {
            get => m_Sender;
            set => m_Sender = value ?? m_Sender;
        }

        /// <summary>
        /// The object that has the property to send.
        /// Must be a non-null type that has the current property
        /// </summary>
        public object? SourceObject
        {
            get => m_SourceObject;
            set => m_SourceObject = value ?? m_SourceObject;
        }

        /// <summary>
        /// The property to send the value of.  Must be a property found on the current SourceComponent.
        /// Will be null if the member being sent is a Field.
        /// </summary>
        public PropertyInfo? Property
        {
            get => m_Property;
            set
            {
                m_MemberInfo = value;
                m_Property = value;
                m_Field = null;
                m_MemberIsProperty = true;
            }
        }

        /// <summary>
        /// The Field to send the value of.  Must be a public field found on the current SourceComponent.
        /// Will be null if the member being sent is a Property.
        /// </summary>
        public FieldInfo? Field
        {
            get => m_Field;
            set
            {
                m_MemberInfo = value;
                m_Field = value;
                m_Property = null;
                m_MemberIsProperty = false;
            }
        }

        public void OnEnable()
        {
            m_HasSender = m_Sender != null;
            SetPropertyFromSerialized();
        }

        //void OnValidate()
        //{
        //    Utils.ValidateAddress(ref m_Address);
        //    if (m_Sender == null) m_Sender = gameObject.GetComponentInParent<OscSender>();
        //    m_HasSender = m_Sender != null;
        //}

        public void Update()
        {
            if (m_MemberInfo == null || !m_HasSender || m_Sender?.Client == null) 
                return;

            var value = m_MemberIsProperty ? m_Property?.GetValue(m_SourceObject) : m_Field?.GetValue(m_SourceObject);
            if (value is null)
                return;
            
            switch (m_PropertyTypeName)
            {
                case "Byte":
                case "SByte": 
                case "Int16":
                case "UInt16":    
                case "Int32":
                    if (ValueChanged(ref m_PreviousIntValue, value, out var intVal))
                        m_Sender.Client.Send(m_Address, intVal);
                    break;
                case "Int64":
                    if (ValueChanged(ref m_PreviousLongValue, value, out var longVal))
                        m_Sender.Client.Send(m_Address, longVal);
                    break;
                case "Single":
                    if (ValueChanged(ref m_PreviousSingleValue, value, out var floatVal))
                        m_Sender.Client.Send(m_Address, floatVal);
                    break;
                case "Double":
                    if (ValueChanged(ref m_PreviousDoubleValue, value, out var doubleVal))
                        m_Sender.Client.Send(m_Address, doubleVal);
                    break;
                case "String":
                    if (ValueChanged(ref m_PreviousStringValue, value, out var stringVal))
                        m_Sender.Client.Send(m_Address, stringVal);
                    break;
                case "Color32":
                    if (ValueChanged(ref m_PreviousColorValue, value, out var colorVal))
                        m_Sender.Client.Send(m_Address, colorVal);
                    break;
                case "Vector2":
                    SendVector2(value);
                    break;
                case "Vector3":
                    SendVector3(value);
                    break;
                case "Boolean":
                    if (ValueChanged(ref m_PreviousBooleanValue, value, out var boolVal))
                        m_Sender.Client.Send(m_Address, boolVal);
                    break;
            }
        }
        
        void SendVector2(object obj)
        {
            var vec = (Vector2) obj;
            switch (m_SendVector2Elements)
            {
                case Vector2ElementFilter.XY:
                    if (!m_PreviousVec2Value.Equals(vec))
                    {
                        m_PreviousVec2Value = vec;
                        m_Sender?.Client?.Send(m_Address, vec);
                    }
                    break;
                case Vector2ElementFilter.X:
                    if (!m_PreviousSingleValue.Equals(vec.X))
                    {
                        m_PreviousSingleValue = vec.X;
                        m_Sender?.Client?.Send(m_Address, vec.X);
                    }
                    break;
                case Vector2ElementFilter.Y:
                    if (!m_PreviousSingleValue.Equals(vec.Y))
                    {
                        m_PreviousSingleValue = vec.Y;
                        m_Sender?.Client?.Send(m_Address, vec.Y);
                    }
                    break;
            }
        }

        void SendVector3(object value)
        {
            var vec = (Vector3) value;

            switch (m_SendVector3Elements)
            {
                case Vector3ElementFilter.XYZ:
                    if (!m_PreviousVec3Value.Equals(vec))
                    {
                        m_PreviousVec3Value = vec;
                        m_Sender?.Client?.Send(m_Address, vec);
                    }
                    break;
                case Vector3ElementFilter.X:
                    if (!m_PreviousSingleValue.Equals(vec.X))
                    {
                        m_PreviousSingleValue = vec.X;
                        m_Sender?.Client?.Send(m_Address, vec.X);
                    }
                    break;
                case Vector3ElementFilter.Y:
                    if (!m_PreviousSingleValue.Equals(vec.Y))
                    {
                        m_PreviousSingleValue = vec.Y;
                        m_Sender?.Client?.Send(m_Address, vec.Y);
                    }
                    break;
                case Vector3ElementFilter.Z:
                    if (!m_PreviousSingleValue.Equals(vec.Z))
                    {
                        m_PreviousSingleValue = vec.Z;
                        m_Sender?.Client?.Send(m_Address, vec.Z);
                    }
                    break;
                case Vector3ElementFilter.XY:
                    var xy = new Vector2(vec.X, vec.Y);
                    if (!m_PreviousVec2Value.Equals(xy))
                    {
                        m_PreviousVec2Value = xy;
                        m_Sender?.Client?.Send(m_Address, xy);
                    }
                    break;
                case Vector3ElementFilter.XZ:
                    var xz = new Vector2(vec.X, vec.Z);
                    if (!m_PreviousVec2Value.Equals(xz))
                    {
                        m_PreviousVec2Value = xz;
                        m_Sender?.Client?.Send(m_Address, xz);
                    }
                    break;
                case Vector3ElementFilter.YZ:
                    var yz = new Vector2(vec.Y, vec.Z);
                    if (!m_PreviousVec2Value.Equals(yz))
                    {
                        m_PreviousVec2Value = yz;
                        m_Sender?.Client?.Send(m_Address, yz);
                    }
                    break;
            }
        }

        static bool ValueChanged<T>(ref T previousValue, object value, out T castValue) where T: IEquatable<T>
        {
            castValue = (T) value;
            if (!castValue.Equals(previousValue))
            {
                previousValue = castValue;
                return true;
            }

            return false;
        }

        internal void SetPropertyFromSerialized()
        {
            if (m_SourceObject == null) 
                return;
            
            var type = m_SourceObject.GetType();

            if (m_MemberIsProperty)
                Property = type?.GetProperty(m_PropertyName);
            else
                Field = type?.GetField(m_PropertyName);
        }
    }
}
