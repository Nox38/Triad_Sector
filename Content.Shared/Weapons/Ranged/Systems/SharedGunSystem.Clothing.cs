using System.Diagnostics.CodeAnalysis;
using Content.Shared.Inventory;
using Content.Shared.Containers;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Weapons.Ranged.Systems;

public partial class SharedGunSystem
{
    [Dependency] private InventorySystem _inventory = default!;

    private void InitializeClothing()
    {
        SubscribeLocalEvent<ClothingSlotAmmoProviderComponent, TakeAmmoEvent>(OnClothingTakeAmmo);
        SubscribeLocalEvent<ClothingSlotAmmoProviderComponent, CheckShootPrototypeEvent>(OnClothingCheckProto); // Mono
        SubscribeLocalEvent<ClothingSlotAmmoProviderComponent, GetAmmoCountEvent>(OnClothingAmmoCount);
    }

    private void OnClothingTakeAmmo(EntityUid uid, ClothingSlotAmmoProviderComponent component, TakeAmmoEvent args)
    {
        var getConnectedContainerEvent = new GetConnectedContainerEvent();
        RaiseLocalEvent(uid, ref getConnectedContainerEvent);
        if(!getConnectedContainerEvent.ContainerEntity.HasValue)
            return;

        RaiseLocalEvent(getConnectedContainerEvent.ContainerEntity.Value, args);
    }

    // Mono
    private void OnClothingCheckProto(Entity<ClothingSlotAmmoProviderComponent> ent, ref CheckShootPrototypeEvent args)
    {
        if (!TryGetClothingSlotEntity(ent, ent.Comp, out var entity))
            return;
        RaiseLocalEvent(entity.Value, ref args);
    }

    private void OnClothingAmmoCount(EntityUid uid, ClothingSlotAmmoProviderComponent component, ref GetAmmoCountEvent args)
    {
        if (!TryGetClothingSlotEntity(uid, component, out var entity))
            return;
        RaiseLocalEvent(entity.Value, ref args);
    }

    private bool TryGetClothingSlotEntity(EntityUid uid, ClothingSlotAmmoProviderComponent component, [NotNullWhen(true)] out EntityUid? slotEntity)
    {
        slotEntity = null;

        if (!Containers.TryGetContainingContainer((uid, null, null), out var container))
            return false;
        var user = container.Owner;

        if (!_inventory.TryGetContainerSlotEnumerator(user, out var enumerator, component.TargetSlot))
            return false;

        while (enumerator.NextItem(out var item))
        {
            if (_whitelistSystem.IsWhitelistFailOrNull(component.ProviderWhitelist, item))
                continue;

            slotEntity = item;
            return true;
        }

        return false;
    }
}
