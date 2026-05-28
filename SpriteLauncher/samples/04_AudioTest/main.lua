-- 04_AudioTest - 音频系统测试
-- 使用 E:/projects/kimi/data/ 下的 WAV 文件

local dataDir = "E:/projects/kimi/data/"

function setup()
    log("info", "Audio Test loaded!")
    log("debug", "Loading sounds from: " .. dataDir)

    -- 加载音效
    loadSound("beep", dataDir .. "beep.wav")
    loadSound("bgm", dataDir .. "bgm.wav")

    -- 播放背景音乐（循环）
    playMusicVol("bgm", 0.3)
    log("info", "BGM started (loop, volume 0.3)")

    -- 在 setup 中直接测试 beep
    log("debug", "Testing beep in setup...")
    playSoundVol("beep", 0.8)
    log("debug", "Beep test done")
end

function update(dt)
    -- 点击鼠标播放 beep 音效
    if mouseIsPressed() then
        log("info", "Mouse pressed! Playing beep...")
        playSoundVol("beep", 0.8)
        log("debug", "Beep played")
    end
end

function draw()
    background(20, 20, 30)

    -- 标题
    fill(255, 255, 255)
    textSize(20)
    text("Audio System Test", 20, 30)

    -- 说明
    fill(200, 200, 200)
    textSize(14)
    text("BGM is playing (loop)", 20, 60)
    text("Click mouse to play 'beep' sound", 20, 80)
    text("Press ESC to exit", 20, 100)

    -- 可视化：点击时的脉冲圆
    if mouseIsPressed() then
        fill(255, 100, 100)
        circle(mouseX(), mouseY(), 30)
    end

    -- 底部状态
    fill(100, 200, 255)
    text("Mouse: (" .. math.floor(mouseX()) .. ", " .. math.floor(mouseY()) .. ")", 20, height() - 40)
    text("Pressed: " .. tostring(mouseIsPressed()), 20, height() - 20)
end
