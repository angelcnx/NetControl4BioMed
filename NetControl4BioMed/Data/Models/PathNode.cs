﻿using NetControl4BioMed.Data.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Models
{
    /// <summary>
    /// Represents the database model of a one-to-one relationship between a path and a node which it contains.
    /// </summary>
    public class PathNode
    {
        /// <summary>
        /// Gets or sets the path ID of the relationship.
        /// </summary>
        public string PathId { get; set; }

        /// <summary>
        /// Gets or sets the path of the relationship.
        /// </summary>
        public Path Path { get; set; }

        /// <summary>
        /// Gets or sets the node ID of the relationship.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the node of the relationship.
        /// </summary>
        public Node Node { get; set; }

        /// <summary>
        /// Gets or sets the type of the relationship.
        /// </summary>
        public PathNodeType Type { get; set; }
    }
}
