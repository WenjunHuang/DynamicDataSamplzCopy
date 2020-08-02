using System;
using System.Collections.ObjectModel;
using System.Security.Principal;
using DynamicData;
using DynamicData.Binding;

namespace DynamicDataSamplzCopy.Examples
{
    public class JoinManyViewModel: AbstractNotifyPropertyChanged,IDisposable
    {
        private readonly IDisposable _cleanUp;
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}