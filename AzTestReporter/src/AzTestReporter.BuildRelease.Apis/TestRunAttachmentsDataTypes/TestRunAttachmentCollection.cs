namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TestRunAttachmentCollection: IList<TestRunAttachmentData>
    {
        public List<TestRunAttachmentData> Attachments;

        public TestRunAttachmentCollection(AzureSuccessReponse testRunAttachments)
        {
            var values = testRunAttachments.Value;
            this.Attachments = new List<TestRunAttachmentData>();
            for (int i = 0; i < testRunAttachments.Count; i++)
            {
                this.Attachments.Add(JsonConvert.DeserializeObject<TestRunAttachmentData>(values[i].ToString()));
            }
        }

        public TestRunAttachmentData this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<Uri> AllCoverageAttachmentUris
        {
            get
            {
                List<Uri> urls = new List<Uri>();
                foreach (TestRunAttachmentData testRunAttachmentData in this.Attachments)
                {
                    if (testRunAttachmentData.FileName.ToUpperInvariant().Contains(".COVERAGEXML"))
                    {
                        urls.Add(testRunAttachmentData.Url);
                    }
                }

                return urls;
            }
        }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(TestRunAttachmentData item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TestRunAttachmentData item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TestRunAttachmentData[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<TestRunAttachmentData> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(TestRunAttachmentData item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TestRunAttachmentData item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TestRunAttachmentData item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
