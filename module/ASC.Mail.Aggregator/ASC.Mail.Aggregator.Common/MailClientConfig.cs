/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code. Version 3.4.0.38967
//    <NameSpace>ASC.Mail.DomainParser</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><EnableLazyLoading>False</EnableLazyLoading><TrackingChangesEnable>False</TrackingChangesEnable><GenTrackingClasses>False</GenTrackingClasses><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>True</EnableSummaryComment><VirtualProp>False</VirtualProp><IncludeSerializeMethod>True</IncludeSerializeMethod><UseBaseClass>True</UseBaseClass><GenBaseClass>True</GenBaseClass><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>False</GenerateDataContracts><CodeBaseTag>Net35</CodeBaseTag><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><GenerateXMLAttributes>True</GenerateXMLAttributes><EnableEncoding>False</EnableEncoding><AutomaticProperties>True</AutomaticProperties><GenerateShouldSerialize>False</GenerateShouldSerialize><DisableDebug>True</DisableDebug><PropNameSpecified>Default</PropNameSpecified><Encoder>UTF8</Encoder><CustomUsings></CustomUsings><ExcludeIncludedTypes>False</ExcludeIncludedTypes><EnableInitializeFields>True</EnableInitializeFields>
//  </auto-generated>
// ------------------------------------------------------------------------------

using System.IO;
using System.Collections.Generic;

namespace ASC.Mail.Aggregator.Common
{

    #region Base entity class
    public class EntityBase<T>
    {

// ReSharper disable StaticFieldInGenericType
        private static System.Xml.Serialization.XmlSerializer _serializer;
// ReSharper restore StaticFieldInGenericType

        private static System.Xml.Serialization.XmlSerializer Serializer
        {
            get
            {
                if ((_serializer == null))
                {
                    _serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                }
                return _serializer;
            }
        }

        #region Serialize/Deserialize

        /// <summary>
        /// Serializes current EntityBase object into an XML document
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memoryStream))
                {
                    return streamReader.ReadToEnd();
                }
            }

        }

        /// <summary>
        /// Serializes current EntityBase object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            var xmlString = Serialize();
            var xmlFile = new FileInfo(fileName);
            using (var streamWriter = xmlFile.CreateText())
            {
                streamWriter.WriteLine(xmlString);
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an EntityBase object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output EntityBase object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out T obj, out System.Exception exception)
        {
            exception = null;
            obj = default(T);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out T obj)
        {
            System.Exception exception;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static T LoadFromFile(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(file))
                {
                    return ((T) (Serializer.Deserialize(System.Xml.XmlReader.Create(sr))));
                }
            }
        }

        #endregion
    }
    #endregion

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3082")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("clientConfig", Namespace = "", IsNullable = false)]
    public class ClientConfig : EntityBase<ClientConfig>
    {
        private ClientConfigEmailProvider _emailProviderField;

        [System.Xml.Serialization.XmlAttributeAttribute("version")]
        public decimal Version { get; set; }

        [System.Xml.Serialization.XmlIgnoreAttribute]
        public bool VersionSpecified { get; set; }

        /// <summary>
        /// clientConfig class constructor
        /// </summary>
        public ClientConfig()
        {
            _emailProviderField = new ClientConfigEmailProvider();
        }

        [System.Xml.Serialization.XmlElementAttribute("emailProvider", Order = 0)]
        public ClientConfigEmailProvider EmailProvider
        {
            get
            {
                return _emailProviderField;
            }
            set
            {
                _emailProviderField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3082")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("clientConfigEmailProvider")]
    public class ClientConfigEmailProvider : EntityBase<ClientConfigEmailProvider>
    {
        private List<string> _domainField;

        private List<ClientConfigEmailProviderIncomingServer> _incomingServerField;

        private List<ClientConfigEmailProviderOutgoingServer> _outgoingServerField;

        private ClientConfigEmailProviderDocumentation _documentationField;

        [System.Xml.Serialization.XmlElementAttribute("displayName", Order = 1)]
        public string DisplayName { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("displayShortName", Order = 2)]
        public string DisplayShortName { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("id")]
        public string Id { get; set; }


        /// <summary>
        /// clientConfigEmailProvider class constructor
        /// </summary>
        public ClientConfigEmailProvider()
        {
            _documentationField = new ClientConfigEmailProviderDocumentation();
            _outgoingServerField = new List<ClientConfigEmailProviderOutgoingServer>();
            _incomingServerField = new List<ClientConfigEmailProviderIncomingServer>();
            _domainField = new List<string>();
        }

        [System.Xml.Serialization.XmlElementAttribute("domain", Order = 0)]
        public List<string> Domain
        {
            get
            {
                return _domainField;
            }
            set
            {
                _domainField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("incomingServer", Order = 3)]
        public List<ClientConfigEmailProviderIncomingServer> IncomingServer
        {
            get
            {
                return _incomingServerField;
            }
            set
            {
                _incomingServerField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("outgoingServer", Order = 4)]
        public List<ClientConfigEmailProviderOutgoingServer> OutgoingServer
        {
            get
            {
                return _outgoingServerField;
            }
            set
            {
                _outgoingServerField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("documentation", Order = 5)]
        public ClientConfigEmailProviderDocumentation Documentation
        {
            get
            {
                return _documentationField;
            }
            set
            {
                _documentationField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3082")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("clientConfigEmailProviderIncomingServer")]
    public class ClientConfigEmailProviderIncomingServer : EntityBase<ClientConfigEmailProviderIncomingServer>
    {
        [System.Xml.Serialization.XmlElementAttribute("hostname", Order = 0)]
        public string Hostname { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("port", Order = 1)]
        public int Port { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("socketType", Order = 2)]
        public string SocketType { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("authentication", Order = 3)]
        public string Authentication { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("username", Order = 4)]
        public string Username { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("type")]
        public string Type { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3082")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("clientConfigEmailProviderOutgoingServer")]
    public class ClientConfigEmailProviderOutgoingServer : EntityBase<ClientConfigEmailProviderOutgoingServer>
    {
        [System.Xml.Serialization.XmlElementAttribute("hostname", Order = 0)]
        public string Hostname { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("port", Order = 1)]
        public int Port { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("socketType", Order = 2)]
        public string SocketType { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("authentication", Order = 3)]
        public string Authentication { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("username", Order = 4)]
        public string Username { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute("type")]
        public string Type { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3082")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("clientConfigEmailProviderDocumentation")]
    public class ClientConfigEmailProviderDocumentation : EntityBase<ClientConfigEmailProviderDocumentation>
    {
        [System.Xml.Serialization.XmlAttributeAttribute("url")]
        public string Url { get; set; }
    }
}
