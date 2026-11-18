extends Control
var lines: RichTextLabel
func _ready():
    var parameters = OS.get_cmdline_user_args()
    lines = RichTextLabel.new()
    lines.position = Vector2(50, 45)
    lines.size = Vector2(900, 550)
    lines.add_theme_font_size_override("normal_font_size", 26)
    add_child(lines)
    lines.text = "GODOT + .NET 11
Actual HTTP game client

Protocol: " + parameters[1] + "
"
    var client = HTTPRequest.new()
    add_child(client)
    var x = 0
    var failed = false
    for turn in range(3):
        var body = JSON.stringify({"version": parameters[1], "x": int(x)})
        client.request(parameters[0] + "/move", ["Content-Type: application/json"], HTTPClient.METHOD_POST, body)
        var response = await client.request_completed
        var status = response[1]
        var text = response[3].get_string_from_utf8()
        lines.text += "
Turn %d | HTTP %d
%s
" % [turn + 1, status, text]
        print("GAME turn ", turn + 1, " status ", status, " body ", text)
        if status != 200:
            failed = true
            break
        x = JSON.parse_string(text)["x"]
    lines.text += "
REJECTED: protocol mismatch" if failed else "
VERIFIED: server position = 3"
    await RenderingServer.frame_post_draw
    get_viewport().get_texture().get_image().save_png(parameters[2])
    get_tree().quit(2 if failed else 0)
