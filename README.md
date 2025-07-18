# 1. Theme 

## 🎨 테마 변경 방법

해당 프로젝트는 컴파일 타임에 테마를 변경해서 빌드 할 수 있습니다.

### 1. 컴파일 타임에 테마 변경

`App.xaml` 파일에서 사용할 테마 파일을 변경

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 기본 테마 (테마 변경시 파일 위치 변경) -->
            <ResourceDictionary Source="Themes/ColorTheme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 2. 새로운 테마 만들기

1. `Themes` 폴더에 새로운 `.xaml` 파일을 생성
2. 기존 테마 파일을 복사해서 컬러 값들을 수정
3. `App.xaml`에서 새 테마 파일 참조

## 🎯 사용 가능한 컬러 리소스

### 기본 컬러
- `PrimaryColor`, `PrimaryDarkColor`, `PrimaryLightColor`
- `SecondaryColor`, `SecondaryDarkColor`, `SecondaryLightColor`
- `AccentColor`, `AccentDarkColor`, `AccentLightColor`

### 배경 컬러
- `BackgroundColor` - 메인 배경
- `SurfaceColor` - 컨테이너 배경
- `CardBackgroundColor` - 카드 배경

### 텍스트 컬러
- `TextPrimaryColor` - 주요 텍스트
- `TextSecondaryColor` - 보조 텍스트
- `TextDisabledColor` - 비활성 텍스트
- `TextOnPrimaryColor` - Primary 배경 위 텍스트

### 테두리 컬러
- `BorderColor` - 기본 테두리
- `BorderFocusColor` - 포커스 테두리
- `BorderHoverColor` - 호버 테두리

### 상태 컬러
- `SuccessColor` - 성공
- `WarningColor` - 경고
- `ErrorColor` - 오류
- `InfoColor` - 정보

### 버튼 컬러
- `ButtonPrimaryBackground`, `ButtonPrimaryHover`, `ButtonPrimaryPressed`
- `ButtonSecondaryBackground`, `ButtonSecondaryHover`, `ButtonSecondaryPressed`

## 💡 사용 예제

XAML에서 테마 컬러 사용하기:`StaticResource`로 Key 사용

```xml
<!-- 텍스트에 Primary 컬러 적용 -->
<TextBlock Text="제목" Foreground="{StaticResource PrimaryColor}"/>

<!-- 버튼에 테마 배경 컬러 적용 -->
<Button Content="저장" Background="{StaticResource ButtonPrimaryBackground}"/>

<!-- Border에 테마 컬러 적용 -->
<Border Background="{StaticResource CardBackgroundColor}" 
        BorderBrush="{StaticResource BorderColor}"/>
```

## 📝 주의사항

- 모든 테마 파일은 동일한 리소스 키를 사용!
- 새로운 컬러를 추가할 때는 모든 테마 파일에 동일한 키로 정의해주세요
- 컬러 값은 헥스 코드 형식 (예: `#2563eb`) 