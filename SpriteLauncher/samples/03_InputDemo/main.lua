-- 03_InputDemo - 展示键盘和鼠标输入

local trail = {}
local maxTrail = 50
local keysPressed = {}

function setup()
    print("Input Demo loaded! Move mouse and press keys.")
end

function update(dt)
    -- 记录鼠标轨迹
    table.insert(trail, 1, { x = mouseX(), y = mouseY() })
    if #trail > maxTrail then
        table.remove(trail)
    end
end

function draw()
    background(15, 15, 25)

    -- 绘制鼠标轨迹
    for i, p in ipairs(trail) do
        local alpha = map(i, 1, maxTrail, 255, 0)
        fill(100, 200, 255, alpha)
        local r = map(i, 1, maxTrail, 12, 2)
        circle(p.x, p.y, r)
    end

    -- 当前鼠标位置
    noStroke()
    fill(255, 100, 120)
    circle(mouseX(), mouseY(), 20)

    -- 信息面板
    fill(30, 30, 40, 200)
    rect(10, 10, 250, 120)
    fill(255)
    textSize(16)
    text("Input Monitor", 20, 30)
    textSize(14)
    text("Mouse X: " .. math.floor(mouseX()), 20, 55)
    text("Mouse Y: " .. math.floor(mouseY()), 20, 75)
    text("Pressed: " .. tostring(mouseIsPressed()), 20, 95)
    text("Trail size: " .. #trail, 20, 115)

    -- 提示
    fill(200, 200, 200)
    textSize(12)
    text("Move mouse to draw trail. Press ESC to exit.", 20, height() - 20)
end
