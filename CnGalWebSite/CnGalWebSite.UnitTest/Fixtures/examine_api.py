"""Local-only fixture API for ExamineBrowserTests. No production data or credentials.
Run: python CnGalWebSite/CnGalWebSite.UnitTest/Fixtures/examine_api.py
"""
import copy
import json
import re
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse

PORT = 5361
MEDIA = f"http://127.0.0.1:{PORT}/fixture-image.svg"
STATE = {}
COUNTS = {}
OPS = {"main": 2, "images": 5, "article": 10, "audio": 24}

def content(new=False):
    return {
        "mainPage": "<h2>故事与角色</h2><p>" + ("补充后的故事介绍。" if new else "原有故事介绍。") * 8 + "</p><table><tr><th>项目</th><th>说明</th></tr><tr><td>世界观</td><td>一次新的旅程</td></tr></table>",
        "texts": [{"modifier": "主要信息", "informations": [{"displayName": "名称", "displayValue": "夏日回响 · 特别篇"}, {"displayName": "简介", "displayValue": "这是一条多行的简介。\n" + "记录每一次相遇。" * 12}, {"displayName": "别称", "displayValue": "Echoes"}, {"displayName": "别称", "displayValue": "夏日回响"}]}],
        "pictures": [{"url": MEDIA, "note": "主视觉 · 2026"}],
        "relevances": [{"entry": {"id": 42, "name": "关联作品"}}, {"tag": {"id": 3, "name": "视觉小说"}}],
        "outlinks": [{"displayName": "官方网站", "link": "https://example.com/" + "long-path/" * 14, "displayValue": "https://example.com/" + "long-path/" * 14}],
        "audio": [{"name": "主题曲试听", "url": f"http://127.0.0.1:{PORT}/fixture-audio.mp3"}]
    }

def record(record_id):
    result = {"id": record_id, "objectId": 42, "objectName": "夏日回响 · 特别篇", "objectBriefIntroduction": "一部关于相遇、成长与夏天的视觉小说。", "image": MEDIA,
        "type": 0, "operation": 2, "userId": "editor-test", "userName": "星海编辑", "applyTime": "2026-11-12T09:30:00", "isAdmin": True,
        "isPassed": None if record_id >= 100 else True, "beforeModel": content(), "afterModel": content(True),
        "editOverview": "<p>补充游戏简介与关联资料，更新主视觉。</p><p><del>原有介绍</del> <ins>补充后的介绍</ins></p>",
        "comments": "", "note": "资料来源：官方发布信息。\n补充内容已核对。", "sensitiveWords": [], "contributionValue": 2}
    if record_id in (102, 103):
        result.update(isPassed=record_id == 102, comments="资料已核对。" if record_id == 102 else "请补充可核验的来源。", passedAdminName="审核员", passedTime="2026-11-12T10:15:00")
    if record_id == 104: result["prepositionExamineId"] = 101
    if record_id == 105: result["isAdmin"] = False
    if record_id == 106:
        result.update(type=4, operation=12, beforeModel={}, afterModel={"mainPage": "<p>这是一条单份评论内容。</p>"})
    if record_id == 110:
        result.update(image=f"http://127.0.0.1:{PORT}/missing.png", beforeModel={}, afterModel={"pictures": [{"url": f"http://127.0.0.1:{PORT}/missing.png", "note": "失效图片"}]})
    if record_id == 111: result.update(beforeModel={}, afterModel={}, editOverview="", note="", comments="")
    if record_id in STATE: result.update(STATE[record_id])
    return result

def overview(record_id):
    ids = [] if record_id == 900 else [record_id] if record_id == 901 else list(range(10, 31))
    return {"objectId": 42, "objectName": "夏日回响 · 特别篇", "type": 0, "objectBriefIntroduction": "一部关于相遇、成长与夏天的视觉小说。", "image": MEDIA,
        "examines": [{"id": i, "userId": "editor-test", "userName": "星海编辑", "operation": 5 if i % 3 == 0 else 2,
            "applyTime": "2026-11-12T09:30:00", "passedTime": "2026-11-12T10:15:00", "passedAdminName": "审核员", "editOverview": f"<p>第 {i} 次修改：补充说明与图片。</p>", "note": "资料来源已核对。"} for i in ids]}

def snapshot(version):
    return {"name": "夏日回响", "briefIntroduction": f"截至记录 #{version} 的简介。", "mainPicture": MEDIA,
        "mainPage": f"<h2>版本 #{version}</h2><p>历史还原内容，与当前审核详情独立。</p>", "anotherName": "Echoes",
        "information": [{"name": "发行时间", "value": "2026 年"}], "staffs": [{"modifier": "制作团队", "staffList": [{"modifier": "编剧", "names": [{"displayName": "创作者"}]}]}],
        "releases": [{"name": "正式版", "engine": "测试引擎", "link": "https://example.com"}],
        "pictures": [{"modifier": "CG", "pictures": [{"url": MEDIA, "note": "场景"}]}],
        "audio": [{"name": "主题曲", "url": f"http://127.0.0.1:{PORT}/fixture-audio.mp3"}]}

class Handler(BaseHTTPRequestHandler):
    def log_message(self, *_): pass
    def send(self, data, status=200, content_type="application/json"):
        raw = data.encode("utf-8") if isinstance(data, str) else json.dumps(data, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", content_type + "; charset=utf-8")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        try: self.wfile.write(raw)
        except (BrokenPipeError, ConnectionResetError): pass
    def do_GET(self):
        path = urlparse(self.path).path.lower()
        if path == "/__fixture": return self.send({"fixture": "cngal-examine-local"})
        if path == "/fixture-image.svg":
            return self.send('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 460 215"><rect width="460" height="215" fill="#dce8f8"/><circle cx="365" cy="65" r="36" fill="#fff4cf"/><path d="M0 175L140 70L255 170L340 120L460 200V215H0Z" fill="#789bb9"/><text x="24" y="195" fill="white" font-size="26">Echoes of Summer</text></svg>', content_type="image/svg+xml")
        if "missing" in path or "fixture-audio" in path: return self.send({}, 404)
        if "getexaminesoverview/" in path:
            record_id = int(path.rsplit("/", 1)[1])
            if record_id == 404: return self.send({}, 404)
            return self.send(overview(record_id))
        if "getexamineview/" in path:
            record_id = int(path.rsplit("/", 1)[1])
            if record_id == 404: return self.send({}, 404)
            if record_id == 109: time.sleep(.5)
            return self.send(record(record_id))
        if "getcontrasteditrecordviews/" in path:
            before, after = map(int, path.split("/")[-2:])
            return self.send({"contrastId": before, "currentId": after, "contrastModel": snapshot(before), "currentModel": snapshot(after)})
        return self.send([])
    def do_POST(self):
        path = urlparse(self.path).path.lower()
        if self.headers.get("Transfer-Encoding", "").lower() == "chunked":
            chunks = []
            while True:
                size = int(self.rfile.readline().strip().split(b";", 1)[0], 16)
                if size == 0:
                    self.rfile.readline()
                    break
                chunks.append(self.rfile.read(size))
                self.rfile.read(2)
            body = b"".join(chunks)
        else:
            body = self.rfile.read(int(self.headers.get("Content-Length", "0")))
        data = {k.lower(): v for k, v in json.loads(body or "{}").items()}
        if path == "/__reset":
            STATE.clear(); COUNTS.clear()
            return self.send({"fixture": "cngal-examine-local"})
        if path == "/api/examines/proc":
            record_id = data["id"]
            COUNTS[record_id] = COUNTS.get(record_id, 0) + 1
            if record_id == 107 and COUNTS[record_id] == 1: return self.send({"successful": False, "error": "模拟网络失败，请重试。"})
            if record_id == 108:
                STATE[record_id] = {"isPassed": True, "comments": "已由其他审核员处理。"}
                return self.send({"successful": False, "error": "该记录已被处理。"})
            STATE[record_id] = {"isPassed": data["ispassed"], "comments": data.get("comments", "")}
            return self.send({"successful": True})
        return self.send({})

if __name__ == "__main__":
    print(f"CnGal examine fixture: http://127.0.0.1:{PORT}", flush=True)
    ThreadingHTTPServer(("127.0.0.1", PORT), Handler).serve_forever()
