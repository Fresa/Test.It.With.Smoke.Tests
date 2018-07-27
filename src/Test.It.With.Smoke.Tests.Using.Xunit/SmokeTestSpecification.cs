using System;
using Test.It.Specifications;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public abstract class SmokeTestSpecification : Specification
    {
        internal void OnSetup()
        {
            Setup();
        }
    }

    public abstract class SmokeTestSpecification<TNext> : SmokeTestSpecification
    where TNext : SmokeTestSpecification
    {
        protected abstract TNext Next();
    }

    public interface INext<TFrom, TTo> 
        where TFrom : SmokeTestSpecification
    where TTo: SmokeTestSpecification
    {
        TTo Next(TFrom from);
    }

    public interface IStart<T> where T : SmokeTestSpecification
    {
        T Start();
    }

    public interface IProcess
    {
        IStart<T> Start<T>() where T : SmokeTestSpecification;
    }

    public interface INext2<TFrom>
        where TFrom : SmokeTestSpecification
    {
        INext2<TTo> Next<TTo>()
            where TTo : SmokeTestSpecification;
    }

    public interface INext<TFrom>
        where TFrom : SmokeTestSpecification
    {
        INext<TTo> Next<TTo>(Func<TFrom, TTo> from)
            where TTo : SmokeTestSpecification;
    }

    public class NextBuilder<TFrom> : INext<TFrom>
        where TFrom : SmokeTestSpecification
    {
        private readonly TFrom _from;

        public NextBuilder(TFrom from)
        {
            _from = @from;
        }

        public INext<TTo> Next<TTo>(Func<TFrom, TTo> from)
            where TTo : SmokeTestSpecification
        {
            var to = from(_from);
            return new NextBuilder<TTo>(to);
        }
    }

    public class NextBuilder2<TFrom> : INext2<TFrom>
        where TFrom : SmokeTestSpecification
    {
        private readonly TFrom _from;

        public NextBuilder2(TFrom from)
        {
            _from = @from;
        }

        public INext2<TTo> Next<TTo>() where TTo : SmokeTestSpecification
        {
            return new NextBuilder2<TTo>(Activator.CreateInstance<TTo>());
        }
    }

    public abstract class Process
    {
        public INext<T> Start<T>(Func<T> fromFunc) 
            where T : SmokeTestSpecification
        {
            var from = fromFunc();
            return new NextBuilder<T>(from);
        }
    }

    public abstract class Process2<TStart>
        where TStart : SmokeTestSpecification
    {
        protected abstract void Start(INext<TStart> fromFunc);
        
        internal void StartInternal()
        {
            Start(new NextBuilder<TStart>(Activator.CreateInstance<TStart>()));
        }
    }

    public abstract class Process3
    {
        public INext2<T> Start<T>()
            where T : SmokeTestSpecification
        {
            return new NextBuilder2<T>(Activator.CreateInstance<T>());
        }
    }


    public interface IFirst<T>
        where T : SmokeTestSpecification
    {

    }

    public interface IAnd<T>
        where T : SmokeTestSpecification
    {

    }

}