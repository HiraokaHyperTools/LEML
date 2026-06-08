using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kenjiuno.LEML
{
    /// <summary>
    /// Mail or multipart entity
    /// </summary>
    public class Mail
    {
        /// <summary>
        /// file path
        /// </summary>
        public String filePath { get; set; }
        /// <summary>
        /// unique id
        /// </summary>
        public String uuid { get; set; }
        /// <summary>
        /// body in latin1
        /// </summary>
        public String rawBody { get; set; }

        /// <summary>
        /// parent entity for multipart/*
        /// </summary>
        public Mail parentEntity { get; set; }

        internal Mail CreateChild(String childRawBody)
        {
            return new Mail { filePath = filePath, uuid = uuid, rawBody = childRawBody, parentEntity = this };
        }

        /// <summary>
        /// Load from file
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns></returns>
        public static Mail FromFile(string filePath)
        {
            return new Mail { filePath = filePath, uuid = filePath, rawBody = System.IO.File.ReadAllText(filePath, Encoding.GetEncoding("latin1")) };
        }

    }
}
