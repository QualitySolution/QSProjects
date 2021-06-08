using NHibernate;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;

namespace QS.Project.DB
{
    public class MySQL57ExtendedDialect : MySQL57Dialect
    {
        public MySQL57ExtendedDialect()
        {
            RegisterFunction("DATE", new StandardSQLFunction("DATE", NHibernateUtil.Date));

            RegisterFunction("GROUP_CONCAT", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"));
            RegisterFunction("GROUP_CONCAT_DISTINCT", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"));
            RegisterFunction("GROUP_CONCAT_ORDER_BY_ASC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 ORDER BY ?2 ASC SEPARATOR ?3)"));
            RegisterFunction("GROUP_CONCAT_ORDER_BY_DESC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 ORDER BY ?2 DESC SEPARATOR ?3)"));
            RegisterFunction("GROUP_CONCAT_DISTINCT_ORDER_BY_ASC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 ORDER BY ?2 ASC SEPARATOR ?3)"));
            RegisterFunction("GROUP_CONCAT_DISTINCT_ORDER_BY_DESC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 ORDER BY ?2 DESC SEPARATOR ?3)"));
            RegisterFunction("CONCAT_WS", new StandardSQLFunction("CONCAT_WS", NHibernateUtil.String));
        }
    }
}
