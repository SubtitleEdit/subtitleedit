using System;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Core
{
    public class AbstractControlDescriptionProvider<TAbstract, TBase> : TypeDescriptionProvider
    {
        public AbstractControlDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(TAbstract)))
        {
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            return objectType == typeof(TAbstract)
                ? typeof(TBase)
                : base.GetReflectionType(objectType, instance);
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            if (objectType == typeof(TAbstract))
            {
                objectType = typeof(TBase);
            }
            return base.CreateInstance(provider, objectType, argTypes, args);
        }
    }
}
