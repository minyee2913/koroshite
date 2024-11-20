-- Packet

-- Example usage:
--
-- ```
--     local net_my_packet = Packet.create()
--     net_my_packet:onReceived(function (message, player_instance_id) 
--         local actor = message:read_instance()
--         local damage_coeff = message:read_float()
--         do_something(actor, damage_coeff)
--     end)
--     
--     ...
--
--     local message = net_my_packet:message_begin()
--     message:write_instance(my_character_actor)
--     message:write_float(0.35)
--     message:send_to_host()
-- ```

Packet = {}

local registered_callbacks = {}
local net_message_onReceived = 41

Packet.TARGET = Proxy.new({
    from_host_to_all = 0,
    from_host_to_client = 1,
    from_host_to_all_except = 2,
    from_client_to_host = 3
}):lock()

local DATA_TYPE = {
    byte = 1,
    short = 2,
    ushort = 3,
    half = 4,
    uhalf = 5,
    int = 6,
    uint = 7,
    float = 8,
    double = 9,
    string = 10,
    instance = 11,
}

local DATA_WRITERS = {}
DATA_WRITERS[DATA_TYPE.byte] = function (buffer, value) GM.writebyte_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.short] = function (buffer, value) GM.writeshort_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.ushort] = function (buffer, value) GM.writeushort_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.half] = function (buffer, value) GM.writehalf_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.uhalf] = function (buffer, value) GM.writeuhalf_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.int] = function (buffer, value) GM.writeint_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.uint] = function (buffer, value) GM.writeuint_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.float] = function (buffer, value) GM.writefloat_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.string] = function (buffer, value) GM.writestring_direct(buffer, value) end
DATA_WRITERS[DATA_TYPE.instance] = function (buffer, value) GM.write_instance_direct(buffer, value) end

Packet.create = function () 
    local uid = GM._mod_net_message_getUniqueID()
    registered_callbacks[uid] = nil
    return {
        uid = uid,

        -- Add a callback to handle messages received for this packet. The function should have 
        -- a signature of `function (message, player_instance_id)`.
        onReceived = function(self, func) 
            registered_callbacks[uid] = func
        end,

        -- Returns a table with a number of functions for providing specific data, to be serialized
        -- into a message and sent via `send` and its derivatives
        message_begin = function(self)
            return {
                net_uid = self.uid,
                data = {},
                write_byte = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.byte,
                        value = value,
                    })
                end,
                write_short = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.short,
                        value = value,
                    })
                end,
                write_ushort = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.ushort,
                        value = value,
                    })
                end,
                write_half = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.half,
                        value = value,
                    })
                end,
                write_uhalf = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.uhalf,
                        value = value,
                    })
                end,
                write_int = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.int,
                        value = value,
                    })
                end,
                write_uint = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.uint,
                        value = value,
                    })
                end,
                write_float = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.float,
                        value = value,
                    })
                end,
                write_string = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.string,
                        value = value,
                    })
                end,
                write_instance = function (self, value)
                    table.insert(self.data, {
                        type = DATA_TYPE.instance,
                        value = value,
                    })
                end,

                send = function(self, target, specific_player) 
                    local buffer = GM._mod_net_message_begin()

                    for _, data in ipairs(self.data) do
                        local writer = DATA_WRITERS[data.type]
                        if writer then 
                            writer(buffer, data.value)
                        else
                            log.error("Unsupported data type in message. It will be ignored")
                            return
                        end
                    end

                    GM._mod_net_message_send(self.net_uid, target, specific_player)
                end,

                send_to_all = function(self)
                    self:send(Packet.TARGET.from_host_to_all, nil)
                end,

                send_direct = function(self, specific_player) 
                    self:send(Packet.TARGET.from_host_to_client, specific_player)
                end,

                send_to_host = function(self)
                    self:send(Packet.TARGET.from_client_to_host, nil)
                end,

                send_exclude = function(self, specific_player) 
                    self:send(Packet.TARGET.from_host_to_all_except, specific_player)
                end,
            }
        end,

    }
end

gm.post_script_hook(gm.constants.callback_execute, function(self, other, result, args)
    local callback_id = args[1].value
    if callback_id == net_message_onReceived then
        local packet_uid = args[2].value
        local on_receive_callback = registered_callbacks[packet_uid]

        if on_receive_callback then
            local player_instance_id = args[5].value

            -- Wrap the buffer in a message object with methods for reading
            local message_reader = {
                buffer = args[3].value,
                read_byte = function (self) return GM.readbyte_direct(self.buffer) end,
                read_short = function (self) return GM.readshort_direct(self.buffer) end,
                read_ushort = function (self) return GM.readushort_direct(self.buffer) end,
                read_half = function (self) return GM.readhalf_direct(self.buffer) end,
                read_uhalf = function (self) return GM.readuhalf_direct(self.buffer) end,
                read_int = function (self) return GM.readint_direct(self.buffer) end,
                read_uint = function (self) return GM.readuint_direct(self.buffer) end,
                read_float = function (self) return GM.readfloat_direct(self.buffer) end,
                read_string = function (self) return GM.readstring_direct(self.buffer) end,
                read_instance = function (self) return GM.read_instance_direct(self.buffer) end,
            }

            -- Call the callback :)
            on_receive_callback(message_reader, player_instance_id)
        end
    end
end)

return NetMessage