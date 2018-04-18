using Sahara.Core.Application.Leads.Models;
using Sahara.Core.Application.Leads.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Leads
{
    /* Account Admin now deals with SALES LEADS DIRECTLY

    internal static class Transformations
    {

        
        public static List<SalesLead> TransformSalesLeadTableEntitiesToSalesLead(IEnumerable<SalesLeadTableEntity> tableEntities)
        {
            var salesLeads = new List<SalesLead>();

            if (tableEntities != null)
            {
                foreach (SalesLeadTableEntity tableEntity in tableEntities)
                {
                    salesLeads.Add(
                        new SalesLead
                        {
                            Timestamp = tableEntity.Timestamp,
                            LeadID = tableEntity.LeadID,

                            Email = tableEntity.Email,
                            Phone = tableEntity.Phone,

                            FirstName = tableEntity.FirstName,
                            LastName = tableEntity.LastName,

                            FullyQualifiedName = tableEntity.FullyQualifiedName,
                            LocationPath = tableEntity.LocationPath,
                            ProductID = tableEntity.ProductID,
                            ProductName = tableEntity.ProductName,

                            Comments = tableEntity.Comments,
                            Notes = tableEntity.Notes,

                            IPAddress = tableEntity.IPAddress,
                            Origin = tableEntity.Origin,
                            Object = tableEntity.Object

                        }
                    );
                }
            }

            return salesLeads;
        }
        
       
    }
    
     */
}
