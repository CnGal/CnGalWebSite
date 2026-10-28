import asyncio
import time
from collections import deque


class InteractionLimiter:
    """同时检查三层滑动窗口，通过后统一计数；被拒绝的请求不占额度。"""
    def __init__(self, clock=time.monotonic, *, window_seconds=60, user_limit=5,
                 session_limit=10, global_limit=60):
        for field, value in {"window_seconds": window_seconds, "user_limit": user_limit,
                             "session_limit": session_limit, "global_limit": global_limit}.items():
            if type(value) is not int or value <= 0:
                raise ValueError(f"rate_limit.{field} 必须为正整数")
        self.window_seconds = window_seconds
        self.user_limit = user_limit
        self.session_limit = session_limit
        self.global_limit = global_limit
        self.clock = clock
        self.lock = asyncio.Lock()
        self.windows = {}
        self.warned_until = {}

    async def admit(self, user, session):
        async with self.lock:
            now = self.clock()
            for key in list(self.windows):
                queue = self.windows[key]
                while queue and queue[0] <= now - self.window_seconds:
                    queue.popleft()
                if not queue:
                    del self.windows[key]
            self.warned_until = {k: t for k, t in self.warned_until.items() if t > now}
            scopes = (("user:" + user, self.user_limit), ("session:" + session, self.session_limit),
                      ("global", self.global_limit))
            for key, count in scopes:
                queue = self.windows.get(key, ())
                if len(queue) >= count:
                    # 同一受限窗口只提醒一次，全局限流也不向每个群重复提醒。
                    warn = key not in self.warned_until
                    self.warned_until[key] = self.warned_until.get(key, queue[0] + self.window_seconds)
                    return False, warn
            for key, _ in scopes:
                self.windows.setdefault(key, deque()).append(now)
            return True, False
