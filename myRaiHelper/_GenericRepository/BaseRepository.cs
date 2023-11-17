using myRai.DataAccess;
using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace myRaiHelper.GenericRepository
{
    /// <summary>
    ///  BaseRepository risponde al problema di forte accoppiamento tra le logiche di business e l’accesso ai dati 
    ///  introducendo un ulteriore livello, per l’appunto il Repository, il quale sarà il solo “responsabile” del recupero e salvataggio dai dati.
    ///  Il repository dovrà avere conoscenza di come sono realmente strutturati i dati  ed assolvere al compito di mappare la classe in esame con la/e tabella/e di destinazione.
    ///  Garantsce la persistenza dei dati: va da se che se il Repository è l’unico responsabile dell’accesso ai dati esso debba necessariamente salvarli da qualche parte e all’occorrenza recuperarli per renderli nuovamente disponibili.
    ///  Espone almeno i metodi CRUD per ogni entità del modello
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseRepository<T> where T : class 
    {
        protected IncentiviEntities db;

        public BaseRepository(IncentiviEntities db)
        {
            this.db = db;           
        }

        public virtual bool Delete(T entityOld)
        {
            bool result;
            try
            {
                if (entityOld == null)
                {
                    return false;
                }

                db.Set<T>().Remove(entityOld);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                //db.SaveChanges();
            }
            catch (Exception ex)
            {
                var msg = string.Empty;
                var fail = new Exception(msg, ex);
                result = false;
            }
            return result;
        }

        public virtual T Get(Expression<Func<T, bool>> expression = null)
        {
            return db.Set<T>().FirstOrDefault(expression);
        }

        public int GeneraOid(Func<T, int> selector) 
        {
            int oid = 0;
            bool trovato = true;
            Random rnd = new Random();

            while (trovato)
            {
                oid = rnd.Next();
                trovato = db.Set<T>().Find(oid) != null;
            }

            return oid;
        }

        public virtual IEnumerable<T> GetAll(
            Func<T,bool> whereFunction = null ,
            Func<T, string> orderAscending = null,
            Func<T,string> thenBy= null,
            Func<T, string> thenBy1 = null,
            Func<T, string> thenBy2 = null,
            Func<T,string> orderDescending = null,
            Func<T,string> thenByDesc = null,
            Func<T, string> thenByDesc1 = null,
            Func<T, string> thenByDesc2 = null) 
        {
            IEnumerable<T> result = db.Set<T>();
            if (whereFunction != null)
                result = result.Where(whereFunction);
            if (orderDescending != null)
            {
                result = result.OrderByDescending(orderDescending);
                if (thenByDesc != null)
                {
                    result = result.OrderByDescending(orderDescending).ThenByDescending(thenByDesc);
                    if(thenByDesc1 != null || thenByDesc2!= null)
                    {
                        result = result.OrderByDescending(orderDescending).ThenByDescending(thenByDesc).ThenByDescending(thenByDesc1).ThenByDescending(thenByDesc2);
                    }
                }
            }
            if (orderAscending != null)
            {
                result = result.OrderBy(orderAscending);
                if (thenBy != null)
                    result = result.OrderBy(orderAscending).ThenBy(thenBy);
                if(thenBy1 != null || thenBy2 != null)
                    result= result.OrderBy(orderAscending).ThenBy(thenBy).ThenBy(thenBy1).ThenBy(thenBy2);
            }
            return result;
        }

        public virtual T GetById(object id) 
        {
            return db.Set<T>().Find(id);
        }

        public virtual IQueryable<T> GetMany(Expression<Func<T, bool>> expression) 
        {
            return db.Set<T>().Where(expression);
        }
        public IQueryable<TType> GetAllSelect<T,TType>(Expression<Func<T, TType>> select) where T : class
        {
            return db.Set<T>().Select(select);
        }
        public virtual bool Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            db.Set<T>().Add(entity);

            return DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());// db.SaveChanges();
        }
        //public void InsertMany(IEnumerable<T> entities) 
        //{
        //    try
        //    {
               
              
        //            if (entities == null)
        //            {
        //                throw new ArgumentNullException("collection");
        //            }
                    
        //            foreach (var item in entities)
        //            {
        //                db.Set<T>().Add(item);
        //            }

                

           
        //    }catch(Exception e)
        //    {
        //        throw e;
        //    }

        //}

        public bool Save() 
        {
            return DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());// db.SaveChanges();
        }
        public virtual bool Update(T entity)
        {
            bool result;
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException("entity");
                }
                db.Set<T>().Attach(entity);
                db.Entry(entity).State = System.Data.EntityState.Modified;
                result = true;
            }
            catch (Exception ex)
            {
                var msg = String.Empty;
                var fail = new Exception(msg, ex);
                result = false;
            }
            return result;
        }
        
    }
    public class ComprelRepository : BaseRepository<COMPREL>
    {
        public ComprelRepository(IncentiviEntities db) : base(db)
        {
        }
    }

}
