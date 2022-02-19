﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Xunit;

namespace Argon.Tests.Documentation.Samples.Serializer;

public class JsonObjectAttributeOptIn : TestFixtureBase
{
    #region Types
    [JsonObject(MemberSerialization.OptIn)]
    public class File
    {
        // excluded from serialization
        // does not have JsonPropertyAttribute
        public Guid Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public int Size { get; set; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region Usage
        var file = new File
        {
            Id = Guid.NewGuid(),
            Name = "ImportantLegalDocuments.docx",
            Size = 50 * 1024
        };

        var json = JsonConvert.SerializeObject(file, Formatting.Indented);

        Console.WriteLine(json);
        // {
        //   "Name": "ImportantLegalDocuments.docx",
        //   "Size": 51200
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""ImportantLegalDocuments.docx"",
  ""Size"": 51200
}", json);
    }
}