namespace Photizer.Domain.Interfaces
{
    public interface IDatabaseManager
    {
        string ExportDatabase();

        string GetDatabaseBackupFolderPath();
    }
}