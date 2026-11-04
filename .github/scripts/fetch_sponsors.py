import hashlib
import json
import os
import time

import requests

USER_ID = os.environ["AFDIAN_USERID"]
TOKEN = os.environ["AFDIAN_TOKEN"]

def generate_sign(params: dict, ts: int) -> str:
    """生成爱发电 API 签名"""
    # 签名规则：md5(token + "params" + params_json + "ts" + ts + "user_id" + user_id)
    params_str = json.dumps(params, separators=(",", ":"), ensure_ascii=False)
    raw = f"{TOKEN}params{params_str}ts{ts}user_id{USER_ID}"
    return hashlib.md5(raw.encode("utf-8")).hexdigest()

def fetch_all_sponsors():
    """循环请求所有分页，返回赞助者列表"""
    all_sponsors = []
    page = 1
    total_page = 1  # 初始值，第一次请求后会更新

    while page <= total_page:
        ts = int(time.time())
        params = {"page": page}
        payload = {
            "user_id": USER_ID,
            "params": json.dumps(params, separators=(",", ":")),
            "ts": ts,
            "sign": generate_sign(params, ts),
        }

        resp = requests.post(
            "https://afdian.net/api/open/query-sponsor",
            json=payload,
            timeout=30,
        )
        data = resp.json()

        if data.get("ec") != 200:
            raise RuntimeError(f"爱发电 API 返回错误: {data}")

        sponsor_data = data["data"]
        total_page = sponsor_data["total_page"]  # 更新总页数
        all_sponsors.extend(sponsor_data["list"])
        print(f"已获取第 {page}/{total_page} 页，累计 {len(all_sponsors)} 位赞助者")
        page += 1

    return all_sponsors

def main():
    raw_list = fetch_all_sponsors()
    # 只保留你需要的字段，保持 JSON 简洁
    cleaned = [
        {
            "name": item["user"]["name"],
            "avatar": item["user"]["avatar"],
            "user_id": item["user"]["user_id"],
        }
        for item in raw_list
    ]

    with open("sponsors.json", "w", encoding="utf-8") as f:
        json.dump(cleaned, f, ensure_ascii=False, indent=2)

    print(f"已将 {len(cleaned)} 位赞助者写入 sponsors.json")

if __name__ == "__main__":
    main()