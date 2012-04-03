﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage.Params;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class StoreWriterTests
    {
        private void TestWriter(IStoreWriter writer, IStoreReader reader, bool useMultiThreaded, int compressionLevel)
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            store.Add(g);
            g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = compressionLevel;
            }
            if (writer is IMultiThreadedWriter)
            {
                ((IMultiThreadedWriter)writer).UseMultiThreadedWriting = useMultiThreaded;
            }
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(store, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Equals(String.Empty));

            TripleStore store2 = new TripleStore();
            reader.Load(store2, new System.IO.StringReader(strWriter.ToString()));

            foreach (IGraph graph in store.Graphs)
            {
                Assert.IsTrue(store2.HasGraph(graph.BaseUri), "Parsed Stored should have contained serialized graph");
                Assert.AreEqual(graph, store2.Graph(graph.BaseUri), "Parsed Graph should be equal to original graph");
            }
        }

        private void TestWriter(IStoreWriter writer, IStoreReader reader, bool useMultiThreaded)
        {
            this.TestWriter(writer, reader, useMultiThreaded, Options.DefaultCompressionLevel);
        }

        [TestMethod]
        public void WritingNQuads()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.WritingNQuadsActual));
        }

        [TestMethod]
        public void WritingNQuadsSingleThreaded()
        {
            this.TestWriter(new NQuadsWriter(), new NQuadsParser(), false);
        }

        private void WritingNQuadsActual()
        {
            this.TestWriter(new NQuadsWriter(), new NQuadsParser(), true);
        }

        [TestMethod]
        public void WritingTriG()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.WritingTriGActual));
        }

        [TestMethod]
        public void WritingTriGSingleThreaded()
        {
            this.TestWriter(new TriGWriter(), new TriGParser(), false);
        }

        private void WritingTriGActual()
        {
            this.TestWriter(new TriGWriter(), new TriGParser(), true);
        }

        [TestMethod]
        public void WritingTriGUncompressed()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.WritingTriGUncompressedActual));
        }

        [TestMethod]
        public void WritingTriGUncompressedSingleThreaded()
        {
            this.TestWriter(new TriGWriter(), new TriGParser(), false, WriterCompressionLevel.None);
        }

        private void WritingTriGUncompressedActual()
        {
            this.TestWriter(new TriGWriter(), new TriGParser(), true, WriterCompressionLevel.None);
        }

        [TestMethod]
        public void WritingTriX()
        {
            this.TestWriter(new TriXWriter(), new TriXParser(), false);
        }
    }
}
