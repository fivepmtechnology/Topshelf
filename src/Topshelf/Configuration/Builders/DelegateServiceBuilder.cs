// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Builders
{
    using System;
    using Runtime;

    public class DelegateServiceBuilder<T> :
        ServiceBuilder
        where T : class
    {
        readonly ServiceFactory<T> _serviceFactory;
        Func<T, HostControl, bool> _continue;
        Func<T, HostControl, bool> _pause;
        Func<T, HostControl, bool> _start;
        Func<T, HostControl, bool> _stop;

        public DelegateServiceBuilder(ServiceFactory<T> serviceFactory, Func<T, HostControl, bool> start,
            Func<T, HostControl, bool> stop, Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue)
        {
            _serviceFactory = serviceFactory;
            _start = start;
            _stop = stop;
            _pause = pause;
            _continue = @continue;
        }

        public ServiceHandle Build(HostSettings settings)
        {
            try
            {
                T service = _serviceFactory(settings);

                return new DelegateServiceHandle(service, _start, _stop, _pause, _continue);
            }
            catch (Exception ex)
            {
                throw new ServiceBuilderException("An exception occurred creating the service: " + typeof(T).Name, ex);
            }
        }

        class DelegateServiceHandle :
            ServiceHandle
        {
            readonly Func<T, HostControl, bool> _continue;
            readonly Func<T, HostControl, bool> _pause;
            readonly T _service;
            readonly Func<T, HostControl, bool> _start;
            readonly Func<T, HostControl, bool> _stop;

            public DelegateServiceHandle(T service, Func<T, HostControl, bool> start, Func<T, HostControl, bool> stop,
                Func<T, HostControl, bool> pause, Func<T, HostControl, bool> @continue)
            {
                _service = service;
                _start = start;
                _stop = stop;
                _pause = pause;
                _continue = @continue;
            }

            public void Dispose()
            {
                var disposable = _service as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            public bool Start(HostControl hostControl)
            {
                return _start(_service, hostControl);
            }

            public bool Pause(HostControl hostControl)
            {
                return _pause(_service, hostControl);
            }

            public bool Continue(HostControl hostControl)
            {
                return _continue(_service, hostControl);
            }

            public bool Stop(HostControl hostControl)
            {
                return _stop(_service, hostControl);
            }
        }
    }
}