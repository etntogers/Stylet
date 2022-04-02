﻿using StyletIoC.Creation;
using System;
using System.Linq.Expressions;
using System.Diagnostics;

namespace StyletIoC.Internal.Creators
{
    /// <summary>
    /// Knows how to create an instance of an abstract factory (generated by Container.GetFactoryForType)
    /// </summary>
    internal class AbstractFactoryCreator : ICreator
    {
        private readonly Type abstractFactoryType;
        public RuntimeTypeHandle TypeHandle => this.abstractFactoryType.TypeHandle;

        public AbstractFactoryCreator(Type abstractFactoryType)
        {
            this.abstractFactoryType = abstractFactoryType;
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            System.Reflection.ConstructorInfo ctor = this.abstractFactoryType.GetConstructor(new[] { typeof(IRegistrationContext) });
            Debug.Assert(ctor != null);
            NewExpression construction = Expression.New(ctor, registrationContext);
            return construction;
        }
    }
}
