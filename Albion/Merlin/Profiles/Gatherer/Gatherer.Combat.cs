﻿using Merlin.API;
using System.Linq;

namespace Merlin.Profiles.Gatherer
{
    public partial class Gatherer
    {
        #region Methods

        public bool HandleAttackers()
        {
            if (_localPlayerCharacterView.IsUnderAttack(out FightingObjectView attacker))
            {
                _localPlayerCharacterView.CreateTextEffect("[Attacked]");

                _state.Fire(Trigger.EncounteredAttacker);
                return true;
            }

            return false;
        }

        public void Fight()
        {
            var player = _localPlayerCharacterView;

            if (player.IsMounted)
            {
                player.MountOrDismount();
                return;
            }

            var spells = player.GetSpells().Ready()
                                .Ignore("ESCAPE_DUNGEON").Ignore("PLAYER_COUPDEGRACE")
                                .Ignore("AMBUSH");

            var attackTarget = player.GetAttackTarget();

            if (attackTarget != null)
            {
                //NOTE: WIP combat, need to include all cases etc :) + maybe some intelligence to it

                var selfBuffSpells = spells.Target(gy.SpellTarget.Self).Category(gy.SpellCategory.Buff);
                if (selfBuffSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("Casting Buff Spell");
                    player.CastOnSelf(selfBuffSpells.FirstOrDefault().SpellSlot);
                    return;
                }

                var selfDamageSpells = spells.Target(gy.SpellTarget.Self).Category(gy.SpellCategory.Damage);
                if (selfDamageSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("Casting Damage Spell");
                    player.CastOnSelf(selfDamageSpells.FirstOrDefault().SpellSlot);
                    return;
                }

                var groundCCSpells = spells.Target(gy.SpellTarget.Ground).Category(gy.SpellCategory.CrowdControl);
                if (groundCCSpells.Any())
                {
                    player.CreateTextEffect("Casting CC Spell");
                    player.CastAt(groundCCSpells.FirstOrDefault().SpellSlot, attackTarget.transform.position);
                    return;
                }

                // TODO: If buffed, don't use channeled spells.

                var enemyDamageSpells = spells.Target(gy.SpellTarget.Enemy).Category(gy.SpellCategory.Damage);
                if (enemyDamageSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("Casting Damage Spell");
                    player.CastOn(enemyDamageSpells.FirstOrDefault().SpellSlot, player.GetAttackTarget());
                    return;
                }

                var enemyBuffSpells = spells.Target(gy.SpellTarget.Enemy).Category(gy.SpellCategory.Buff);
                if (enemyBuffSpells.Any() && !player.IsCastingSpell())
                {
                    player.CreateTextEffect("Casting Damage Spell");
                    player.CastOn(enemyBuffSpells.FirstOrDefault().SpellSlot, player.GetAttackTarget());
                    return;
                }

                /*
				var selfDamageSpells = spells.Target(gs.SpellTarget.Self).Category(gs.SpellCategory.Damage);
				if (selfDamageSpells.Any())
				{
				}

				*/
            }

            if (player.IsUnderAttack(out FightingObjectView attacker))
            {
                player.SetSelectedObject(attacker);
                player.AttackSelectedObject();
                return;
            }

            if (player.IsCasting())
                return;

            if (player.GetHealth() < (player.GetMaxHealth() * 0.8f))
            {
                var healSpell = spells.Target(gy.SpellTarget.Self).Category(gy.SpellCategory.Heal);

                if (healSpell.Any())
                    player.CastOnSelf(healSpell.FirstOrDefault().SpellSlot);

                return;
            }

            _currentTarget = null;
            _harvestPathingRequest = null;

            _state.Fire(Trigger.EliminatedAttacker);

        }
            #endregion Methods
    }
}