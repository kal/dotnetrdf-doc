/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Represents common extensions that are useful across all Plugin libraries.
    /// </summary>
    static class DataExtensions
    {
        /// <summary>
        /// Gets either the String form of the Object of the Empty String.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Result of calling <strong>ToString()</strong> on non-null objects and the empty string for null objects.</returns>
        internal static String ToSafeString(this Object obj)
        {
            return (obj != null ? obj.ToString() : String.Empty);
        }

        /// <summary>
        /// Ensures that a specific Object Factory type is registered in a Configuration Graph.
        /// </summary>
        /// <param name="context">Configuration Serialization Context.</param>
        /// <param name="factoryType">Factory Type.</param>
        internal static void EnsureObjectFactory(this ConfigurationSerializationContext context, Type factoryType)
        {
            INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
            INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
            var assm = Assembly.GetAssembly(factoryType).FullName;
            if (assm.Contains(',')) assm = assm.Substring(0, assm.IndexOf(','));

            //Firstly need to ensure our object factory has been referenced
            INode typeNode = context.Graph.CreateLiteralNode(factoryType.FullName + ", " + assm);
            INode objectFactoryNode =
                context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassObjectFactory));
            IEnumerable<INode> existingRegistrations = 
                from factory in context.Graph.GetTriplesWithPredicateObject(dnrType, typeNode)
                join objectFactory in context.Graph.GetTriplesWithPredicateObject(rdfType, objectFactoryNode)
                    on factory.Subject equals objectFactory.Subject
                select factory.Subject;
            if (!existingRegistrations.Any())
            {
                INode factory = context.Graph.CreateBlankNode();
                context.Graph.Assert(new Triple(factory, rdfType, objectFactoryNode));
                context.Graph.Assert(new Triple(factory, dnrType, typeNode));
            }
        }
    }
}
