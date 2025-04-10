namespace UserService.Db.Entities
{
    public abstract class Document: BaseDeletableEntity
    {
        public DocumentType documentType { get; set; }
    }
}
