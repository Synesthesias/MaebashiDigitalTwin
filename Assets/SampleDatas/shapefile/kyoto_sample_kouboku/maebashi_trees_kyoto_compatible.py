import geopandas as gpd
import pandas as pd
import numpy as np
from shapely.geometry import Point
import math

def create_maebashi_trees_kyoto_compatible():
    """
    前橋駅前けやき並木通りの街路樹データを京都データと完全互換形式で作成
    """
    print("=== 前橋駅前街路樹データ作成（京都互換版）===")
    
    # 京都データを読み込んで構造を確認
    print("\n--- 京都データ構造の確認 ---")
    kyoto_gdf = gpd.read_file("kouboku_itibu.shp", encoding='utf-8')
    print(f"京都データのカラム: {kyoto_gdf.columns.tolist()}")
    print(f"京都データのCRS: {kyoto_gdf.crs}")
    print(f"京都データの最初の5行:")
    print(kyoto_gdf.head())
    
    # 前橋駅の正確な座標
    maebashi_station = {
        'lat': 36.38319,  # 正確な前橋駅の緯度
        'lng': 139.07322  # 正確な前橋駅の経度
    }
    
    # けやき並木通りの実際の方向（真北へ）
    road_angle = 0  # 真北方向（前橋駅北口から真北に延びる）
    
    # 街路樹データを作成
    trees_data = []
    
    # 路線名の定義（前橋用に変更）
    rosenmei = "前橋駅前通"  # 実際の路線名
    
    # OBJECTIDの開始値（京都データの続きから）
    object_id = 3507  # 京都データの最後のIDの次から
    
    # 配置計画
    # けやき並木通りの道路中心に沿って配置
    sidewalk_spacing = 15  # 15m間隔
    total_length = 500  # 500m
    
    # 道路中心線に沿った配置パターン
    # 1. 道路中心から少しずらした配置（実際の街路樹配置を模擬）
    offsets = [
        {'name': '西側歩道内側', 'offset': -8, 'kubun': '歩道'},
        {'name': '西側歩道外側', 'offset': -12, 'kubun': '歩道'},
        {'name': '中央分離帯西', 'offset': -2, 'kubun': '中央分離帯'},
        {'name': '中央分離帯東', 'offset': 2, 'kubun': '中央分離帯'},
        {'name': '東側歩道内側', 'offset': 8, 'kubun': '歩道'},
        {'name': '東側歩道外側', 'offset': 12, 'kubun': '歩道'}
    ]
    
    for offset_info in offsets:
        offset = offset_info['offset']
        kubun = offset_info['kubun']
        
        # 配置間隔を調整（中央分離帯は密に、歩道は標準）
        if kubun == '中央分離帯':
            spacing = 20
        else:
            spacing = sidewalk_spacing
            
        for distance in range(0, total_length + 1, spacing):
            # 道路に沿った座標計算
            angle_rad = math.radians(road_angle)
            
            # 道路に沿った移動
            lat_offset = (distance * math.cos(angle_rad)) / 111000
            lng_offset_along = (distance * math.sin(angle_rad)) / (111000 * math.cos(math.radians(maebashi_station['lat'])))
            
            # 道路に垂直な移動（オフセット）
            perpendicular_angle = angle_rad + math.pi / 2
            lat_offset_perp = (offset * math.cos(perpendicular_angle)) / 111000
            lng_offset_perp = (offset * math.sin(perpendicular_angle)) / (111000 * math.cos(math.radians(maebashi_station['lat'])))
            
            new_lat = maebashi_station['lat'] + lat_offset + lat_offset_perp
            new_lng = maebashi_station['lng'] + lng_offset_along + lng_offset_perp
            
            # 樹種の決定（区間と位置により変化）
            if distance < 100:  # 駅前100m
                if distance % 60 == 0 and distance > 0:
                    jushumei = "サクラ"  # 駅前はサクラでアクセント
                else:
                    jushumei = "ケヤキ"
            elif distance < 300:  # 商業区間
                if distance % 45 == 30:
                    jushumei = "イチョウ"
                elif distance % 90 == 60:
                    jushumei = "モミジ"
                else:
                    jushumei = "ケヤキ"
            else:  # 300m以降
                if distance % 60 == 30:
                    jushumei = "イチョウ"
                else:
                    jushumei = "ケヤキ"
            
            # 幹周（樹種と位置により調整）
            if jushumei == "ケヤキ":
                mikishu = np.random.randint(40, 80)
            elif jushumei == "サクラ":
                mikishu = np.random.randint(35, 65)
            else:
                mikishu = np.random.randint(30, 60)
            
            tree_data = {
                'OBJECTID': object_id,
                'ROSENMEI_G': rosenmei,
                'KUBUN': kubun,
                'JUSHUMEI': jushumei,
                'JUKOU': "",  # 空欄（京都データと同じ）
                'MIKISHU': mikishu,
                'geometry': Point(new_lng, new_lat)
            }
            
            trees_data.append(tree_data)
            object_id += 1
    
    # 特別な配置：駅前広場のシンボルツリー
    special_trees = [
        {'distance': 30, 'offset': 0, 'jushumei': 'ケヤキ', 'mikishu': 120, 'kubun': 'シンボル'},
        {'distance': 50, 'offset': -5, 'jushumei': 'サクラ', 'mikishu': 80, 'kubun': 'シンボル'},
        {'distance': 50, 'offset': 5, 'jushumei': 'サクラ', 'mikishu': 80, 'kubun': 'シンボル'}
    ]
    
    for tree in special_trees:
        angle_rad = math.radians(road_angle)
        
        lat_offset = (tree['distance'] * math.cos(angle_rad)) / 111000
        lng_offset_along = (tree['distance'] * math.sin(angle_rad)) / (111000 * math.cos(math.radians(maebashi_station['lat'])))
        
        perpendicular_angle = angle_rad + math.pi / 2
        lat_offset_perp = (tree['offset'] * math.cos(perpendicular_angle)) / 111000
        lng_offset_perp = (tree['offset'] * math.sin(perpendicular_angle)) / (111000 * math.cos(math.radians(maebashi_station['lat'])))
        
        new_lat = maebashi_station['lat'] + lat_offset + lat_offset_perp
        new_lng = maebashi_station['lng'] + lng_offset_along + lng_offset_perp
        
        tree_data = {
            'OBJECTID': object_id,
            'ROSENMEI_G': rosenmei,
            'KUBUN': tree['kubun'],
            'JUSHUMEI': tree['jushumei'],
            'JUKOU': "",
            'MIKISHU': tree['mikishu'],
            'geometry': Point(new_lng, new_lat)
        }
        
        trees_data.append(tree_data)
        object_id += 1
    
    # GeoDataFrame作成（京都と同じCRS）
    gdf_maebashi = gpd.GeoDataFrame(trees_data, crs=kyoto_gdf.crs)
    
    # ファイル出力（前橋データのみ）
    print(f"\n--- ファイル出力 ---")
    
    # 前橋データのみ
    output_maebashi = "maebashi_keyaki_namiki.shp"
    gdf_maebashi.to_file(output_maebashi, encoding='utf-8')
    print(f"✓ 前橋データ: {output_maebashi} (UTF-8)")
    
    # CSV出力（確認用）
    csv_output = "maebashi_keyaki_namiki.csv"
    df_csv = gdf_maebashi.drop('geometry', axis=1)
    df_csv.to_csv(csv_output, index=False, encoding='utf-8')
    print(f"✓ CSV: {csv_output} (UTF-8)")
    
    # 統計情報
    print(f"\n=== 前橋データ統計 ===")
    print(f"総樹木数: {len(gdf_maebashi)}本")
    print(f"\n樹種別:")
    print(gdf_maebashi['JUSHUMEI'].value_counts())
    print(f"\n区分別:")
    print(gdf_maebashi['KUBUN'].value_counts())
    print(f"\n幹周統計:")
    print(f"  最小: {gdf_maebashi['MIKISHU'].min()}cm")
    print(f"  最大: {gdf_maebashi['MIKISHU'].max()}cm")
    print(f"  平均: {gdf_maebashi['MIKISHU'].mean():.1f}cm")
    
    # 座標確認
    print(f"\n座標範囲:")
    bounds = gdf_maebashi.total_bounds
    print(f"  西: {bounds[0]:.6f}")
    print(f"  南: {bounds[1]:.6f}")
    print(f"  東: {bounds[2]:.6f}")
    print(f"  北: {bounds[3]:.6f}")
    print(f"\n前橋駅座標: {maebashi_station['lat']:.6f}, {maebashi_station['lng']:.6f}")
    
    return gdf_maebashi


if __name__ == "__main__":
    print("=" * 70)
    print("前橋駅前けやき並木通り 街路樹データ作成")
    print("京都データ互換版（UTF-8）")
    print("=" * 70)
    
    # データ作成
    gdf_maebashi = create_maebashi_trees_kyoto_compatible()
    
    print("\n" + "=" * 70)
    print("✅ 作成完了！")
    print("📁 出力ファイル:")
    print("  - maebashi_keyaki_namiki.shp (UTF-8)")
    print("  - maebashi_keyaki_namiki.csv (UTF-8)")
    print("📍 配置: 前橋駅（36.38319, 139.07322）から真北へ500m")
    print("🌳 けやき並木通り沿いに配置")
    print("🔤 エンコーディング: UTF-8")
    print("📊 フィールド: OBJECTID, ROSENMEI_G, KUBUN, JUSHUMEI, JUKOU, MIKISHU")
    print("=" * 70)
