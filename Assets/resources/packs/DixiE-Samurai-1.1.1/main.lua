-- TODO
-- Final mp testing

log.info("Loading ".._ENV["!guid"]..".")
mods["RoRRModdingToolkit-RoRR_Modding_Toolkit"].auto() 
NetMessage = require("./packet")

local PATH = _ENV["!plugins_mod_folder_path"]
local DIXIE = "DixiE"

local initialise = function() 
    local samurai = Survivor.new(DIXIE, "samurai")

    -- Utility function for getting sprite paths concisely
    local load_sprite = function (id, filename, frames, orig_x, orig_y, speed, left, top, right, bottom) 
        local sprite_path = path.combine(PATH, "Sprites",  filename)
        return Resources.sprite_load(DIXIE, id, sprite_path, frames, orig_x, orig_y, speed, left, top, right, bottom)
    end

    -- Load the common survivor sprites into a table
    local sprites = {
        idle = load_sprite("samurai_idle", "sSamuraiIdle.png", 5, 18,  22),
        walk = load_sprite("samurai_walk", "sSamuraiRun.png", 8, 15, 19),
        jump = load_sprite("samurai_jump", "sSamuraiJump.png", 1, 48, 69),
        jump_peak = load_sprite("samurai_jump_peak", "sSamuraiJumpPeak.png", 1, 48, 69),
        fall = load_sprite("samurai_fall", "sSamuraiJumpFall.png", 1, 48, 69),
        climb = load_sprite("samurai_climb", "sSamuraiWallContact.png", 3, 48, 55),
        climb_hurt = load_sprite("samurai_climb_hurt", "sSamuraiHurt.png", 3, 48, 69), 
        death = load_sprite("samurai_death", "sSamuraiDeath.png", 10, 48, 69),
        decoy = load_sprite("samurai_decoy", "sSamuraiIdle.png", 5, 48, 69),
    }

    -- Skill sprites
    local spr_atk1 = load_sprite("samurai_atk1", "sSamuraiAtk1.png", 5, 48, 69)
    local spr_atk2 = load_sprite("samurai_atk2", "sSamuraiAtk2.png", 5, 48, 69)
    local spr_atk3 = load_sprite("samurai_atk3", "sSamuraiAtk3.png", 5, 48, 69)
    local spr_defend = load_sprite("samurai_defend", "sSamuraiDefend.png", 6, 48, 69)
    local spr_defend_counter = load_sprite("samurai_defend_counter", "sSamuraiDefendCounter.png", 6, 381, 69) 
    local spr_throw = load_sprite("samurai_throw", "sSamuraiThrow.png", 7, 48, 69)
    local spr_shuriken = load_sprite("samurai_shuriken", "sSamuraiShuriken.png", 3, 6, 6, 0.25, 2, 2, 9, 9) 
    local spr_shuriken_cloned = load_sprite("samurai_shuriken_cloned", "sSamuraiShurikenCloned.png", 4, 6, 6, 0.25, 2, 2, 9, 9) 
    local spr_drink = load_sprite("samurai_drink", "sSamuraiHeal.png", 15, 48, 69)

    -- Icons, portraits, etc.
    local spr_skills = load_sprite("samurai_skills", "iSamuraiSkills.png", 5, 0, 0) 
    local spr_portrait = load_sprite("samurai_portrait", "iSamuraiPortrait.png", 3)
    local spr_portrait_small = load_sprite("samurai_portrait_small", "iSamuraiPortraitSmall.png") 
    local spr_loadout = load_sprite("samurai_loadout", "iSamuraiLoadout.png", 4, 28, 0) 
    local spr_palette = load_sprite("samurai_palettes", "iSamuraiPalettes.png")
    local spr_slash_hit = load_sprite("samurai_slash_hit", "eSamuraiSlashHit.png", 1, 17, 5, 0.4)  
    local spr_drunk = load_sprite("samurai_drunk", "iSamuraiDrunk.png", 1, 8, 8)  
    local spr_mega_drunk = load_sprite("samurai_drunk_upgrade", "iSamuraiDrunkUpgrade.png", 1, 8, 8) 
    
    -- Alternate palette art
    local spr_portrait_PAL1 = load_sprite("samurai_portrait_PAL1", "iSamuraiPortrait_PAL1.png", 3)
    local spr_portrait_small_PAL1 = load_sprite("samurai_portrait_small_PAL1", "iSamuraiPortraitSmall_PAL1.png") 
    local spr_loadout_PAL1 = load_sprite("samurai_loadout_PAL1", "iSamuraiLoadout_PAL1.png", 4, 28, 0) 
    local spr_portrait_PAL2 = load_sprite("samurai_portrait_PAL2", "iSamuraiPortrait_PAL2.png", 3)
    local spr_portrait_small_PAL2 = load_sprite("samurai_portrait_small_PAL2", "iSamuraiPortraitSmall_PAL2.png") 
    local spr_loadout_PAL2 = load_sprite("samurai_loadout_PAL2", "iSamuraiLoadout_PAL2.png", 4, 28, 0) 

    -- Colour for the character's skill names on character select
    samurai:set_primary_color(Color.from_rgb(150, 150, 180))

    -- Assign sprites to various survivor fields
    samurai.sprite_loadout = spr_loadout
    samurai.sprite_portrait = spr_portrait
    samurai.sprite_portrait_small = spr_portrait_small
    samurai.sprite_title = sprites.walk
    samurai.sprite_idle = sprites.idle
    samurai.sprite_credits = spr_drink
    samurai:set_palettes(spr_palette, spr_palette, spr_palette)
    samurai:set_animations(sprites)

    -- Add alternate colours
    samurai:add_skin("samurai_alt_1", 1, spr_loadout_PAL1, spr_portrait_PAL1, spr_portrait_small_PAL1)
    samurai:add_skin("samurai_alt_2", 2, spr_loadout_PAL2, spr_portrait_PAL2, spr_portrait_small_PAL2)

    -- Offset for the cape visual
    samurai:set_cape_offset(-1, -6, 0, -5)

    -- Survivor stats
    samurai:set_stats_base({
        maxhp = 120,
        damage = 14,
        regen = 0.01
    })

    samurai:set_stats_level({
        maxhp = 24,
        damage = 2,
        regen = 0.002,
        armor = 4
    })

    -- Data for the primary skill combo. Indexes are floats because reasons
    local combo_atks = {}
    combo_atks[0.0] = {
        sprite = spr_atk1,
        hstep = 2,
        box = { 27, -10, 54, 35 },
    }
    combo_atks[1.0] = {
        sprite = spr_atk2,
        hstep = 2,
        box = { 28, -14, 56, 43 },
    }
    combo_atks[2.0] = {
        sprite = spr_atk3,
        hstep = 2,
        box = { 27, -14, 54, 44 },
    }

    -- Drunk buff
    local buff_drunk = Buff.new(DIXIE, "samurai_drunk")
    buff_drunk.max_stack = 5
    buff_drunk.is_timed = true
    buff_drunk.show_icon = true
    buff_drunk.draw_stack_number = true
    buff_drunk.icon_sprite = spr_drunk

    -- Mega Drunk buff for ancient scepter
    local buff_mega_drunk = Buff.new(DIXIE, "samurai_mega_drunk")
    buff_mega_drunk.max_stack = 9
    buff_mega_drunk.is_timed = true
    buff_mega_drunk.show_icon = true
    buff_mega_drunk.draw_stack_number = true
    buff_mega_drunk.icon_sprite = spr_mega_drunk

    -- Shuriken bleed, implemented via an Object rather than a buff because it is easier to carry 
    -- state and source the damage to a parent actor
    local obj_shuriken_bleed = Object.new(DIXIE, "samurai_shuriken_bleed")
    obj_shuriken_bleed:onStep(function(inst)
        local data = inst:get_data()
        if data.counter == nil then data.counter = 0 end

        -- Handle the case where the target no longer exists
        if data.target == nil or data.target.dead == nil then
            inst:destroy()
            return
        end

        data.counter = data.counter + 1
        if data.counter == 60 then
            data.ticks = data.ticks - 1
            data.counter = 0
            local damager = data.parent:fire_direct(data.target, data.damage_coeff, 0, nil, nil, gm.constants.sNone)
            damager.knockback_kind = 0 -- No knockback
            damager:set_critical(false)
            damager:set_proc(false)
            damager:set_color(Color.from_rgb(100, 0, 0))
        end

        if data.ticks <= 0 then
            inst:destroy()
            return 
        end
    end)

    -- Shuriken objects
    local obj_shuriken = Object.new(DIXIE, "samurai_shuriken")
    obj_shuriken.obj_sprite = spr_shuriken
    obj_shuriken.obj_depth = 1

    local obj_shuriken_cloned = Object.new(DIXIE, "samurai_shuriken_cloned")
    obj_shuriken_cloned.obj_sprite = spr_shuriken_cloned
    obj_shuriken_cloned.obj_depth = 1

    function shurikenStep(inst) 
        local data = inst:get_data()

        inst.x = inst.x + data.hspeed
        inst.y = inst.y + data.vspeed

        -- Hit the first enemy actor that's been collided with
        local actor_collisions, _ = inst:get_collisions(gm.constants.pActorCollisionBase)
        for _, actor in ipairs(actor_collisions) do
            if data.parent:attack_collision_canhit(actor) then
                -- Deal damage
                local dmg = data.damage_coeff 
                local dmg_direction = data.angle
                for i=1, data.clone_multiplier do -- Multiple hits per clone, boosting damage and rerolling procs
                    data.parent:fire_direct(actor, dmg, data.angle, inst.x, inst.y, gm.constants.sEfSlash)
                end

                -- Apply a bleed debuff to the enemy, which actually involves instantiating
                -- an object and configuring values on it
                local inst_bleed = obj_shuriken_bleed:create(actor.x, actor.y)
                local bleed = inst_bleed:get_data()
                bleed.target = actor.value
                bleed.parent = data.parent
                bleed.ticks = 4
                bleed.damage_coeff = 0.35  * data.clone_multiplier

                -- Destroy the shuriken
                inst:destroy()
                return
            end
        end

        -- Hitting terrain destroys the shuriken
        if inst:is_colliding(gm.constants.pSolidBulletCollision) then
            inst:destroy()
            return
        end

        -- Check we're within stage bounds
        local stage_width = GM._mod_room_get_current_width()
        local stage_height = GM._mod_room_get_current_height()
        if inst.x < -16 or inst.x > stage_width + 16 
           or inst.y < -16 or inst.y > stage_height + 16 
        then 
            inst:destroy()
            return
        end
    end

    obj_shuriken:onStep(shurikenStep)
    obj_shuriken_cloned:onStep(shurikenStep)

    -- Grab references to default skills
    local skill_cut = samurai:get_primary(1)
    local skill_throw = samurai:get_secondary(1)
    local skill_parry = samurai:get_utility(1)
    local skill_drink = samurai:get_special(1)

    -- Create alternate parry skill
    local skill_parry_alt = Skill.new(DIXIE, "samuraiCAlt")
    samurai:add_utility(skill_parry_alt)

    -- Set skill animations
    skill_cut:set_skill_animation(spr_atk1)
    skill_throw:set_skill_animation(spr_throw)
    skill_parry:set_skill_animation(spr_defend)
    skill_parry_alt:set_skill_animation(spr_defend)
    skill_drink:set_skill_animation(spr_drink)

    -- Create states for each skill
    local state_cut = State.new(DIXIE, skill_cut.identifier)
    local state_throw = State.new(DIXIE, skill_throw.identifier)
    local state_parry = State.new(DIXIE, skill_parry.identifier)
    local state_parry_alt = State.new(DIXIE, skill_parry_alt.identifier)
    local state_parry_attack = State.new(DIXIE, skill_parry.identifier.."_attack")
    local state_drink = State.new(DIXIE, skill_drink.identifier)

    -- Configure icons for each skill
    skill_cut:set_skill_icon(spr_skills, 0)
    skill_throw:set_skill_icon(spr_skills, 1)
    skill_parry:set_skill_icon(spr_skills, 2)
    skill_parry_alt:set_skill_icon(spr_skills, 4)
    skill_drink:set_skill_icon(spr_skills, 3)

    -- Configure damage and cooldown for each skill
    skill_cut:set_skill_properties(1.5, 0)
    skill_throw:set_skill_properties(0.6, 4*60)
    skill_parry:set_skill_properties(0.7, 7*60)
    skill_parry_alt:set_skill_properties(1.2, 7*60)
    skill_drink:set_skill_properties(0, 30*60)

    -- Initialise some callbacks
    local parry_callback = nil
    samurai:onInit(function (inst) 
        if parry_callback == nil then
            log.warning("parry_callback is nil")
        else
            if inst:callback_exists("samurai_parry_callback") then
                inst:remove_callback("samurai_parry_callback")
            end
            inst:add_callback("onDamageBlocked", "samurai_parry_callback", parry_callback)
        end
    end)

    samurai:onStep(function (inst) 
        local data = inst:get_data()
        if data.combo_counter == nil then data.combo_counter = 0 end
        if data.combo_hit == nil then data.combo_hit = 0 end

        if data.combo_counter > 0 then
            data.combo_counter = data.combo_counter - 1
        else 
            data.combo_hit = 0
        end
    end)

    -- Primary skill handlers
    skill_cut:onActivate(function(actor, skill, index) 
        GM.actor_set_state(actor, state_cut)
    end)

    state_cut:onEnter(function(actor, data) 
        local actor_data = actor:get_data()
        actor.image_index = 0
        data.fired = 0
        data.current_hit = math.floor(actor_data.combo_hit % 3)
        actor_data.combo_hit = actor_data.combo_hit + 1
        actor_data.combo_counter = 60 * 2 -- 2 seconds and the combo resets
    end)

    state_cut:onStep(function(actor, data) 
        actor:skill_util_fix_hspeed()
        local atk = combo_atks[data.current_hit]
        actor:actor_animation_set(atk.sprite, 0.25)

        local direction = GM.cos(GM.degtorad(actor:skill_util_facing_direction()))

        if data.fired == 0 and actor.image_index >= 2 then
            local dmg = actor:skill_get_damage(skill_cut)
            if actor:is_grounded() then
                actor.pHspeed = direction * atk.hstep
            end
            
            if not actor:skill_util_update_heaven_cracker(actor, dmg) then
                local buff_shadow_clone = Buff.find("ror", "shadowClone")
                for i=0, actor:buff_stack_count(buff_shadow_clone) do
                    local x_offset = atk.box[1] * direction
                    actor:fire_explosion(actor.x + x_offset, actor.y + atk.box[2], atk.box[3], atk.box[4], dmg, gm.constants.sNone, spr_slash_hit)
                end
            end

            actor:sound_play(gm.constants.wMercenaryShoot1_1, 1, 0.8 + math.random() * 0.2)
            data.fired = 1
        end

        actor:skill_util_exit_state_on_anim_end()
    end)


    -- Secondary skill handlers
    skill_throw:onActivate(function(actor, skill, index) 
        GM.actor_set_state(actor, state_throw)
    end)

    state_throw:onEnter(function(actor, data) 
        actor.image_index = 0
        data.fired = 0
    end)

    state_throw:onStep(function(actor, data) 
        actor:skill_util_fix_hspeed()
        actor:actor_animation_set(actor:actor_get_skill_animation(skill_throw), 0.25)

        if data.fired == 0 and actor.image_index >= 2 then
            local dmg = actor:skill_get_damage(skill_throw)
            local shuriken_speed = 10
            local drunk_stacks = actor:buff_stack_count(buff_drunk)
            local mega_drunk_stacks = actor:buff_stack_count(buff_mega_drunk)
            if mega_drunk_stacks > drunk_stacks then
                drunk_stacks = mega_drunk_stacks
            end
            local buff_shadow_clone = Buff.find("ror", "shadowClone")
            local clone_stacks = actor:buff_stack_count(buff_shadow_clone)
            local angle = actor:skill_util_facing_direction()
            local direction = GM.cos(GM.degtorad(angle)) 
            local spawn_x = actor.x + direction * 10
            local spawn_y = actor.y - 3
            
            -- Make the spread nice for multiple shurikens
            local spread_per_shuriken = 5
            local spread = drunk_stacks * spread_per_shuriken
            angle = angle - spread / 2

            -- Spawn a shuriken with a different sprite if we have clone stacks
            local shuriken_obj = obj_shuriken
            if clone_stacks > 0 then
                shuriken_obj = obj_shuriken_cloned
            end

            -- The loop that actually spawns the shurikens
            for i=0, drunk_stacks do
                local shuriken_inst = shuriken_obj:create(spawn_x, spawn_y)
                local shuriken_data = shuriken_inst:get_data()
                shuriken_data.parent = actor
                shuriken_data.vspeed = shuriken_speed * GM.sin(GM.degtorad(angle))
                shuriken_data.hspeed = shuriken_speed * GM.cos(GM.degtorad(angle))
                shuriken_data.damage_coeff = dmg
                shuriken_data.clone_multiplier = clone_stacks + 1
                shuriken_data.angle = angle
                angle = angle + spread_per_shuriken
            end

            actor:sound_play(gm.constants.wHuntressShoot1, 1, 0.8 + math.random() * 0.2)
            data.fired = 1
        end

        actor:skill_util_exit_state_on_anim_end()
    end)

    -- Utility skill functions that are used by both regular and alt parry
    local init_parry = function (actor, data, parry_window)
        actor.image_index = 0
        data.parried = 0
        data.stall_counter = parry_window - 20
        actor:set_immune(parry_window + 4)
        actor:sound_play(gm.constants.wClayShoot1, 1, 0.8 + math.random() * 0.2)
    end

    local step_parry = function (actor, data) 
        actor:skill_util_fix_hspeed()
        local animation_speed = 0.25

        -- We don't want attack speed to speed up the parry itself, because that could end up
        -- reducing the parry window, so we undo its benefit ahead of time
        if actor.attack_speed > 0 then
            animation_speed = animation_speed / actor.attack_speed
        end

        actor:actor_animation_set(actor:actor_get_skill_animation(skill_parry), animation_speed)

        -- In order to extend the parry duration, we want to stall on the later frames 
        if actor.image_index >= 5 and data.stall_counter > 0 then
            actor.image_index = 4
            data.stall_counter = data.stall_counter - (1 / animation_speed)
        end

        actor:skill_util_exit_state_on_anim_end()
    end

    -- Utility skill handlers
    skill_parry:onActivate(function(actor, skill, index) 
        actor:enter_state(state_parry)
    end)

    state_parry:onEnter(function(actor, data) 
        init_parry(actor, data, 40)
    end)

    state_parry:onStep(function(actor, data) 
        step_parry(actor, data)
    end)

    -- Alt utility skill handlers
    skill_parry_alt:onActivate(function(actor, skill, index)
        actor:enter_state(state_parry_alt)
    end)

    state_parry_alt:onEnter(function (actor, data) 
        init_parry(actor, data, 20)
    end)

    state_parry_alt:onStep(function (actor, data)
        step_parry(actor, data)
    end)

    -- MP support for the parry is kinda spicy, we want to make sure the change in state is 
    -- propagated to all players so they see the cool animation and not just the damage.
    -- The only way to do this right now while letting us pass around specific data is with 
    -- a custom network message
    local net_counter_packet = Packet.create()
    net_counter_packet:onReceived(function (message, player_instance_id)
        local parry_dmg = message:read_float()
        local actor = message:read_instance()
        if actor then 
            activate_counter(actor, parry_dmg)

            -- If we're the host then we should forward this message to all the other clients
            -- except the one who initially sent it to us
            local net_type = Net.get_type()
            if net_type == Net.TYPE.host then
                local counter_msg = net_counter_packet:message_begin()
                counter_msg:write_float(parry_dmg)
                counter_msg:write_instance(actor)
                counter_msg:send_exclude(player_instance_id)
            end
        end
    end)

    function activate_counter(actor, parry_dmg) 
        local actor_data = actor:get_data()
        actor_data.parry_dmg = parry_dmg
        actor:sound_play(gm.constants.wMercenary_Parry_Deflection, 1, 0.8 + math.random() * 0.2)
        actor:enter_state(state_parry_attack)
    end

    -- Callback to enter the parry attack state when successfully blocked an attack
    parry_callback = function(actor, damager)
        local current_state = GM.actor_get_current_actor_state(actor)
        if current_state == state_parry.value or current_state == state_parry_alt.value then
            local state_data = GM.actor_get_actor_state_data(actor)

            if state_data.parried == 0 then
                state_data.parried = 1 -- Don't allow consecutive parries with a single usage


                local parry_dmg = actor:skill_get_damage(skill_parry)
                if current_state == state_parry_alt.value then
                    parry_dmg = actor:skill_get_damage(skill_parry_alt)
                end
                activate_counter(actor, parry_dmg)

                -- Send network message to activate the counter
                local counter_msg = net_counter_packet:message_begin()
                counter_msg:write_float(parry_dmg)
                counter_msg:write_instance(actor)
                local net_type = Net.get_type()
                if net_type == Net.TYPE.host then
                    counter_msg:send_to_all()
                elseif net_type == Net.TYPE.client then
                    counter_msg:send_to_host()
                end
            end
        end
    end

    state_parry_attack:onEnter(function(actor, data)
        -- Frame 0 is a hacky sprite to make things look good when repeatedly flipping, so we start
        -- at frame 1 instead
        actor.image_index = 1 
        data.fired = 0
        data.teleported = 0
        data.parry_direction = GM.cos(GM.degtorad(actor:skill_util_facing_direction()))

        -- Counters for repeating the attack with drunk stacks
        data.drunk_repeats = 0 
        data.total_drunk_repeats = actor:buff_stack_count(buff_drunk)
        local mega_drunk_stacks = actor:buff_stack_count(buff_mega_drunk)
        if mega_drunk_stacks > data.total_drunk_repeats then
            data.total_drunk_repeats = mega_drunk_stacks
        end

        -- Hold their initial y-position and prevent them from moving from it
        data.parry_y_position = actor.y
    end)

    state_parry_attack:onStep(function(actor, data)
        actor:skill_util_fix_hspeed()
        actor:actor_animation_set(spr_defend_counter, 0.4)

        actor.y = data.parry_y_position
        actor.pVspeed = 0.0

        -- Teleport to the end of the slash, using hspeed to maintain collision detection
        if actor.image_index >= 1 then
            if data.teleported == 0 then
                data.teleported = 1 
                local teleport_offset = data.parry_direction * 200 
                actor.pHspeed = teleport_offset
                data.parry_direction = data.parry_direction * -1
            else
                actor.pHspeed = 0
                if data.fired == 0 then
                    -- Reset cooldowns 
                    actor:refresh_skill(Skill.SLOT.primary)
                    actor:refresh_skill(Skill.SLOT.secondary)
                    actor:refresh_skill(Skill.SLOT.special)

                    -- Apply bonus immunity to prevent coming out of the attack and being insta-gibbed
                    actor:set_immune(45)

                    -- Make an explosion that deals damage around the samurai
                    local buff_shadow_clone = Buff.find("ror", "shadowClone")
                    for i=0, actor:buff_stack_count(buff_shadow_clone) do
                        local explosion_offset = data.parry_direction * 100
                        local actor_data = actor:get_data()
                        local parry_dmg = actor_data.parry_dmg

                        -- This is nil for all clients other than the player controlling this samurai
                        if parry_dmg then
                            actor:fire_explosion(actor.x + explosion_offset, actor.y - 10, 300, 20, parry_dmg, nil, spr_slash_hit)
                        end
                    end

                    actor:sound_play(gm.constants.wMercenaryShoot1_3, 1, 0.8 + math.random() * 0.2)
                    data.fired = 1
                end
            end
        end


        -- Repeat the slash for each drunk stack 
        if data.drunk_repeats < data.total_drunk_repeats and actor.image_index >= 5 then
            data.teleported = 0
            data.fired = 0
            actor.image_xscale = actor.image_xscale * -1 -- Flip the sprite
            actor.image_index = 0 -- Frame 0 is a hack to account for being flipped this frame
            data.drunk_repeats = data.drunk_repeats + 1
        end

        actor:skill_util_exit_state_on_anim_end()
    end)

    -- Special skill handlers
    skill_drink:onActivate(function(actor, skill, index) 
        GM.actor_set_state(actor, state_drink)
    end)

    state_drink:onEnter(function(actor, data) 
        actor.image_index = 0
        data.fired = 0
    end)

    state_drink:onStep(function(actor, data) 
        actor:skill_util_fix_hspeed()
        actor:actor_animation_set(actor:actor_get_skill_animation(skill_drink), 0.25)

        if data.fired == 0 and actor.image_index >= 7 then
            data.fired = 1
            actor:sound_play(gm.constants.wEgg, 1, 0.8 + math.random() * 0.2)
            local scepter = Item.find("ror", "ancientScepter")
            local has_scepter = (actor:item_stack_count(scepter) > 0)
            local drunk_stack_count = actor:buff_stack_count(buff_drunk)
            local total_stacks = drunk_stack_count
            if has_scepter then
                if drunk_stack_count > 0 then
                    actor:buff_remove(buff_drunk, drunk_stack_count)
                    actor:buff_apply(buff_mega_drunk, 30*60, drunk_stack_count)
                end

                total_stacks = actor:buff_stack_count(buff_mega_drunk)
                actor:buff_apply(buff_mega_drunk, 30*60)
            else
                actor:buff_apply(buff_drunk, 20*60)
            end
        end

        actor:skill_util_exit_state_on_anim_end()
    end)

    log.info("Finished initialising samurai")
end


Initialize(initialise)
