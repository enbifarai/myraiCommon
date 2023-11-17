using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcNInject.Model;
namespace MvcNInject.Interface
{
    public interface IEvent
    {
        void Add(Event _Event);     // Create New Event    
        void Update(Event _Event);  // Modify Event    
        void Delete(Event _Event);  // Delete Event    
        Event GetById(int id); // Get an Single Event details by id    
        IEnumerable<Event> GetAll();  // Gets All Event details    
    }
}
