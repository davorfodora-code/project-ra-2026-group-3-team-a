namespace CommunityHub.Application.Domain;

public class AssemblyAttendance
{
    public long Id { get; private set; }
    public TenantAssembly Assembly { get; private set; }
    public int ApartmentNumber { get; private set; }
    public User Tenant { get; private set; }

    public AssemblyAttendance(long id, TenantAssembly assembly, int apartmentNumber, User tenant)
    {
        Id = id;
        Assembly = assembly;
        ApartmentNumber = apartmentNumber;
        Tenant = tenant;
    }

    public AssemblyAttendance(TenantAssembly assembly, int apartmentNumber, User tenant)
    {
        Id = 0;
        Assembly = assembly;
        ApartmentNumber = apartmentNumber;
        Tenant = tenant;
    }
}
