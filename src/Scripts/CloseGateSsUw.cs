namespace Vinki;
public class CloseGateSsUw : UpdatableAndDeletable
{
    public CloseGateSsUw(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        (this.room.regionGate as ElectricGate).batteryLeft = 0f;
        this.Destroy();
    }
}
