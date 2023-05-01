using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models
{
    public class AuthorDto
    {
        /// <summary>
        /// Id of the author
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the author
        /// </summary>
        public string Name { get; set; } 

        /// <summary>
        /// Author's age
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// MainCategory is taken by the author 
        /// </summary>
        public string MainCategory { get; set; }
    }
}
