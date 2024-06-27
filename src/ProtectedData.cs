using System;
using System.Reflection;

//Standard set of functions to access private things

namespace CQaCP {
  public class ProtectedData {
    public static object GetPrivateField(object instance, string fieldName)
    {
      return GetPrivateFieldInternal(instance, fieldName, instance.GetType());

      static object GetPrivateFieldInternal(object instance, string fieldName, Type type)
      {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != (FieldInfo) null)
          return field.GetValue(instance);
        return type.BaseType != (Type) null ? GetPrivateFieldInternal(instance, fieldName, type.BaseType) : (object) null;
      }
    }

    public static object GetPrivateProperty(object instance, string propertyName)
    {
      return GetPrivatePropertyInternal(instance, propertyName, instance.GetType());

      static object GetPrivatePropertyInternal(object instance, string propertyName, Type type)
      {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (property != (PropertyInfo) null)
          return property.GetValue(instance);
        return type.BaseType != (Type) null ? GetPrivatePropertyInternal(instance, propertyName, type.BaseType) : (object) null;
      }
    }

    public static void SetPrivateField(object instance, string fieldName, object value)
    {
      SetPrivateFieldInternal(instance, fieldName, value, instance.GetType());

      static void SetPrivateFieldInternal(
        object instance,
        string fieldName,
        object value,
        Type type)
      {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != (FieldInfo) null)
        {
          field.SetValue(instance, value);
        }
        else
        {
          if (!(type.BaseType != (Type) null))
            return;
          SetPrivateFieldInternal(instance, fieldName, value, type.BaseType);
        }
      }
    }

    //UNTESTED
    public static void SetPrivateProperty(object instance, string propertyName, object value)
    {
      SetPrivatePropertyInternal(instance, propertyName, value, instance.GetType());

      static void SetPrivatePropertyInternal(
        object instance,
        string propertyName,
        object value,
        Type type)
      {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (property != (PropertyInfo) null)
        {
          property.SetValue(instance, value);
        }
        else
        {
          if (!(type.BaseType != (Type) null))
            return;
          SetPrivatePropertyInternal(instance, propertyName, value, type.BaseType);
        }
      }
    }

    public static void CallPrivateMethod(object instance, string methodName, object[] parameters)
    {
      CallPrivateMethodInternal(instance, methodName, parameters, instance.GetType());

      static void CallPrivateMethodInternal(
        object instance,
        string methodName,
        object[] parameters,
        Type type)
      {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method != (MethodInfo) null)
        {
          method.Invoke(instance, parameters);
        }
        else
        {
          if (!(type.BaseType != (Type) null))
            return;
          CallPrivateMethodInternal(instance, methodName, parameters, type.BaseType);
        }
      }
    }
    
    //UNTESTED
    public static void CallPrivateGenericMethod(object instance, string methodName, object[] parameters, Type genericType)
    {
      CallPrivateMethodInternal(instance, methodName, parameters, instance.GetType(), genericType);

      static void CallPrivateMethodInternal(
        object instance,
        string methodName,
        object[] parameters,
        Type type,
        Type genericType)
      {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method != (MethodInfo) null)
        {
          MethodInfo generic = method.MakeGenericMethod(genericType);
          generic.Invoke(instance, parameters);
        }
        else
        {
          if (!(type.BaseType != (Type) null))
            return;
          CallPrivateMethodInternal(instance, methodName, parameters, type.BaseType, genericType);
        }
      }
    }
  }
}
