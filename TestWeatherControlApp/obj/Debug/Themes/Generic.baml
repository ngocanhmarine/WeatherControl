   M S B A M L   `   `   `  ����I  EWeatherControl, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null[>clr-namespace:WeatherControl.Converter;assembly=WeatherControlWeatherControl.Converter  5      P LTestWeatherControlApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null_4clr-namespace:TestWeatherControlApp.MyWeatherControl&TestWeatherControlApp.MyWeatherControl 5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089=   7TestWeatherControlApp.MyWeatherControl.MyWeatherControl  �� 
TargetType �� Property �� Value �� 
TargetType   keyFile  keyExit  keyLanguage  
keyEnglish
 � Tag m� 	IsChecked  keyVietnamese � 	Resources0   *WeatherControl.Converter.TempUnitConverter  Key0   *WeatherControl.Converter.AqiColorConverter �� Orientation � ColumnDefinitions	 � RowDefinitions  keyDescription  keyPressure  keyHumidity 	 
keyTempMax 
 
keyTempMin  keyVisibility  keyWindSpeed  keyWindDegree
 �� Mode �� BindsDirectlyToSource  tempUnitConverter �� 	Converter�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      L	converter>clr-namespace:WeatherControl.Converter;assembly=WeatherControl  5      >local4clr-namespace:TestWeatherControlApp.MyWeatherControl  5      %�  '         7  7  �� 5      "    6   .�5   
   ��  � �� 5	      �� 5
      "   6"   .R�5      �� #��z��6   #��z��5      #��z��5      ._�5      ]� ��.  $
��1000��6:   .+�5      ]� $	��Top=�6(   .+�5   "   o� $	��Top=�6'   .@�5   &   m� $��menuFile��6R   #���   6/   .@�5   *   m� $��menuExit��6V   #���  63   6V   5   '   m� 5   &   $��languageChoice��6V   #���  6/   .@�5   *   m� $��English��6Y   #���  63   $ en-US��6h    . 6t   m� 5   *   $��
Vietnamese��6\   #���  63   $ vi-VN��6n   5   '   5   #   �� 5   "   $��iconImg��6=   $	��Top=�6(   $
��Auto��6L   $��50��6Y   5      �� 5      $��	txtCities��6G   $��50��6&   $	��Top=�62   $��Hanoi��6X   � 5      $��wplWeatherInfos��6J   $	��210��6(   $	��Top=�65   $
��Auto��6a   .+�5   "   �� $��	lvWeather��6+    5   &   ��  5    *   tempUnitConverter 6b     5!   *   aqiColorConverter 6b   5"   '   ��5#   &   �� 5$   *   .R�5%   .   �� $ 
Horizontal=�69   6T   5'   +   ��5*   &   �� 5+   *   .R�5,   .   �� 
�������
�������$��1q�6e   ._�5-   2   � $��5q�67   
������� 5.   6   �� 5/   :   $��* �6K   �� 50   :   $
��Auto �6K   �� 51   :   $
��Auto �6K   	 53   6   �� 54   :   $��* �6H   �� 55   :   $��* �6H   �� 56   :   $��* �6H   �� 57   :   $��* �6H   �� 58   :   $��* �6H   �� 59   :   $��* �6H   �� 5:   :   $��* �6H   �� 5;   :   $��* �6H   �� 5<   :   $��* �6H   .+�5>   6   �� $��1��6@   $��0��6M   #���  6]   �� 5?   6   $��2��6@   $��0��6M   #���  6]   �� 5@   6   $��3��6@   $��0��6M   #���  6]   �� 5A   6   $��tempMax��6]   $��4��6@   $��0��6M   #��� 	 6l   �� 5B   6   $��tempMin��6]   $��5��6@   $��0��6M   #��� 
 6l   �� 5C   6   $��6��6@   $��0��6M   #���  6]   �� 5D   6   $��7��6@   $��0��6M   #���  6]   �� 5E   6   $��8��6@   $��0��6M   #���  6]   �� 5G   6   $��0��6@   $��2��6M   $��Center=�6a   $
��Bold"�6~   $��30(�6�   ��6�   �� *city+$
 TwoWay=� . �� 5H   6   $��1��6@   $��1��6M   ��6]   �� *description+$
 TwoWay=� . �� 5I   6   $��2��6@   $��1��6M   ��6]   �� *
pressure+$
 TwoWay=� . �� 5J   6   $��3��6@   $��1��6M   ��6]   �� *
humidity+$
 TwoWay=� . �� 5K   6   $��temp_max��6�   $��4��6@   $��1��6M   ��6]   �� *
temp_max+$
 TwoWay=� . 6�   �� 5L   6   $��temp_min��6�   $��5��6@   $��1��6M   ��6]   �� *
temp_min+$
 TwoWay=� . 6�   �� 5M   6   $��6��6@   $��1��6M   ��6]   �� *
visibility+$
 TwoWay=� . �� 5N   6   $��7��6@   $��1��6M   ��6]   �� *
wind_speed+$
 TwoWay=� . �� 5O   6   $��8��6@   $��1��6M   ��6]   �� *
wind_deg+$
 TwoWay=� . �� 5Q   6   $��pressureUnit��6]   $��2��6@   $��2��6M   $	��hPa��6q   �� 5R   6   $��humidityUnit��6]   $��3��6@   $��2��6M   $��%��6q   �� 5S   6   $��tempMaxUnit��6]   $��4��6@   $��2��6M   ��6p   �� *	isCelcius+8   �� 5T   6   $��tempMinUnit��6]   $��5��6@   $��2��6M   ��6p   �� *	isCelcius+8  �� 5U   6   $��6��6@   $��2��6M   $�� ��6]   �� 5V   6   $��windSpeedUnit��6]   $��7��6@   $��2��6M   $
��km/h��6r   �� 5W   6   $��windDegUnit��6]   $��8��6@   $��2��6M   $��°��6p   5X   3   5Y   /   5Z   +   5\   #   5]      � 5_      $��
wplButtons��6G   $��10q�6#   $��Bottom=�6/    5`   "   �� 5a   &   $��* �67   �� 5b   &   $��* �67   �� 5c   &   $��* �67   �� 5d   &   $��* �67   .+�5f   "   �� $��btnAdd��6F   $��0��6)   $
��Auto��69   .��6T   Add26Z   �� 5g   "   $��btnEdit��6F   $��1��6)   $
��Auto��69   .��6U   Edit26\   �� 5h   "   $��	btnDelete��6F   $��2��6)   $
��Auto��69   .��6W   	Remove26`   �� 5i   "   $��btnClear��6F   $��3��6)   $
��Auto��69   .��6V   Clear26^   5j      5k      5l      5m      5o      5p      5q      